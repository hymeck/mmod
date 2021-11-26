import argparse
import math
import sys
import simpy
from numpy.random import exponential as exp
from numpy import ndarray
from functools import reduce
from typing import Optional, Union


class QueueingSystemParameters:
    def __init__(self, arrival_rate: float, service_rate: float, server_count: int, queue_capacity: int, v: float,
                 duration: float):
        self.arrival_rate = arrival_rate
        self.service_rate = service_rate
        self.server_count = server_count
        self.queue_capacity = queue_capacity
        self.v = v
        self.duration = duration


class StatisticsHolder:
    def __init__(self):
        self.times_in_system: list[float] = []
        self.times_in_queue: list[float] = []
        self.customers_in_system: list[int] = []
        self.customers_in_queue: list[int] = []
        self.served_customers: list[int] = []
        self.rejected_customers = 0


class QueueingSystem:
    def __init__(self, holder: QueueingSystemParameters, env: simpy.Environment):
        self.holder = holder
        self.env = env
        self.server = simpy.Resource(env, holder.server_count)
        self.statistics = StatisticsHolder()

    def _simulate_customer_arrival(self): return self.env.timeout(exp(1 / self.holder.arrival_rate))

    def _get_customer_patience_generator(self): yield self.env.timeout(exp(1 / self.holder.v))

    def _get_server_processing_generator(self): yield self.env.timeout(exp(1 / self.holder.service_rate))

    @property
    def _waiting_customers(self) -> int: return len(self.server.queue)

    @property
    def _served_customers(self) -> int: return self.server.count

    def _serve_customer(self):
        in_queue_before_request = self._waiting_customers
        in_server_before_request = self._served_customers

        self.statistics.customers_in_queue.append(in_queue_before_request)
        self.statistics.customers_in_system.append(in_queue_before_request + in_server_before_request)

        with self.server.request() as request:
            in_queue_after_request = self._waiting_customers
            in_server_after_request = self._served_customers

            if in_queue_after_request <= self.holder.queue_capacity:
                arrival_time = self.env.now
                self.statistics.served_customers.append(in_queue_after_request + in_server_after_request)
                yield request | self.env.process(self._get_customer_patience_generator())
                waiting_time = self.env.now - arrival_time
                self.statistics.times_in_queue.append(waiting_time)
                if request.processed:
                    yield self.env.process(self._get_server_processing_generator())
                time_in_system = self.env.now - arrival_time
                self.statistics.times_in_system.append(time_in_system)

            else:
                self.statistics.rejected_customers = self.statistics.rejected_customers + 1
                self.statistics.times_in_queue.append(0)
                self.statistics.times_in_system.append(0)

    def _generate_customers(self):
        while True:
            self.env.process(self._serve_customer())
            yield self._simulate_customer_arrival()

    def _mean(self, source: list) -> float:
        return sum(source) / len(source)

    @property
    def probabilities(self) -> list[float]:
        n = self.holder.server_count
        m = self.holder.queue_capacity
        rejected = self.statistics.rejected_customers
        total_count = len(self.statistics.served_customers) + rejected
        last = rejected / total_count
        return [self.statistics.served_customers.count(i) / total_count for i in range(1, n + m + 1)] + [last]

    @property
    def rejection_probability(self) -> float: return self.probabilities[-1]

    @property
    def relative_bandwidth(self) -> float: return 1 - self.rejection_probability

    @property
    def absolute_bandwidth(self) -> float: return self.holder.arrival_rate * self.relative_bandwidth

    @property
    def average_customers_in_queue(self) -> float: return self._mean(self.statistics.customers_in_queue)

    @property
    def average_customers_in_system(self) -> float: return self._mean(self.statistics.customers_in_system)

    @property
    def average_time_in_queue(self) -> float: return self._mean(self.statistics.times_in_queue)

    @property
    def average_time_in_system(self) -> float: return self._mean(self.statistics.times_in_system)

    @property
    def average_busy_servers(self) -> float: return self.average_customers_in_system

    def run(self):
        self.env.process(self._generate_customers())
        self.env.run(until=self.holder.duration)
        return self


def setup_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser()
    parser.add_argument('-d', '--duration', help='simulation time (used in simpy.Environment.run())', type=int,
                        default=5000)
    parser.add_argument('-ar', '--arrival-rate', help='arrival rate mean', type=float, default=10.)
    parser.add_argument('-sr', '--service-rate', help='service rate mean', type=float, default=3.)
    parser.add_argument('-sc', '--server-count', help='number of servers in a system', type=int, default=3)
    parser.add_argument('-qc', '--queue-capacity', help='capacity of a queue', type=int, default=4)
    parser.add_argument('-v', '--v', help='v', type=float, default=6.)
    parser.add_argument('-start', help='start value for stationary mode plots', type=int, default=100)
    parser.add_argument('-end', help='end value for stationary mode plots', type=int, default=10001)
    parser.add_argument('-step', help='step value for stationary mode plots', type=int, default=100)
    return parser


def get_args(parser: argparse.ArgumentParser) -> argparse.Namespace:
    try:
        args = parser.parse_args()
        return args
    except:
        return sys.exit(1)


def print_input(input_holder: QueueingSystemParameters) -> None:
    print(f'Arrival rate: {input_holder.arrival_rate}')
    print(f'Service rate: {input_holder.service_rate}')
    print(f'Server count: {input_holder.server_count}')
    print(f'Queue capacity: {input_holder.queue_capacity}')
    print(f'v: {input_holder.v}')
    print(f'Investigation duration: {input_holder.duration}')
    print()


def get_input_parameters(args: argparse.Namespace) -> QueueingSystemParameters:
    return QueueingSystemParameters(args.arrival_rate, args.service_rate, args.server_count, args.queue_capacity,
                                    args.v, args.duration)


class TheoreticalStatistics:
    def __init__(self, parameters: QueueingSystemParameters):
        self.parameters = parameters
        self.n = parameters.server_count
        self.m = parameters.queue_capacity
        self.mu = parameters.service_rate
        self.lamb = parameters.arrival_rate
        self.rho = self.lamb / self.mu
        self.v = parameters.v
        self.beta = self.v / self.mu
        self.p0: Optional[float] = None
        self._probabilities: Optional[list[float]] = None

    def _prob_factor1(self, k: int) -> float: return (self.rho ** k) / math.factorial(k)

    def _prob_factor2(self, i: int) -> float: return (self.rho ** i) / reduce(lambda product, l: product * (self.n + l * self.beta), range(1, i + 1), 1.)

    def _p0(self) -> float:
        if self.p0 is None:
            rho, n, m = self.rho, self.n, self.m
            sum1 = sum(self._prob_factor1(k) for k in range(1, n + 1))
            factor = self._prob_factor1(n)
            sum2 = sum(self._prob_factor2(i) for i in range(1, m + 1))
            self.p0 = 1. / (1 + sum1 + factor * sum2)
        return self.p0

    @property
    def probabilities(self) -> list[float]:
        if self._probabilities is None:
            p0 = self._p0()
            pk = [p0 * self._prob_factor1(k) for k in range(1, self.n + 1)]
            pn = pk[-1]
            p_n_i = [pn * self._prob_factor2(i) for i in range(1, self.m + 1)]
            self._probabilities = [p0] + pk + p_n_i
        return self._probabilities

    @property
    def rejection_probability(self) -> float: return self.probabilities[-1]

    @property
    def relative_bandwidth(self) -> float: return 1 - self.rejection_probability

    @property
    def absolute_bandwidth(self) -> float: return self.parameters.arrival_rate * self.relative_bandwidth

    @property
    def average_customers_in_queue(self) -> float: return sum(i * self.probabilities[self.n + i] for i in range(1, self.m + 1))

    @property
    def average_customers_in_system(self) -> float:
        return sum((i + 1) * self.probabilities[1:][i] for i in range(0, self.n)) + \
               sum((self.n + i + 1) * self.probabilities[self.n + 1:][i] for i in range(0, self.m))

    @property
    def average_time_in_queue(self) -> float: return self.average_customers_in_queue / self.parameters.arrival_rate

    @property
    def average_time_in_system(self) -> float: return self.average_customers_in_system / self.parameters.arrival_rate

    @property
    def average_busy_servers(self) -> float: return self.average_customers_in_system


def print_statistics(statistics: Union[QueueingSystem, TheoreticalStatistics], theoretical: bool = True) -> None:
    header = 'theoretical' if theoretical else 'empirical'
    print(f'{header} statistics')
    print(f'probabilities:')
    for i, p in enumerate(statistics.probabilities):
        print(f'#{i}: {p}')
    print()
    print(f'sum: {sum(statistics.probabilities)}')
    print(f'probability of rejection: {statistics.rejection_probability}')
    print(f'relative bandwidth: {statistics.relative_bandwidth}')
    print(f'absolute bandwidth: {statistics.absolute_bandwidth}')
    print(f'average number of customers in queue: {statistics.average_customers_in_queue}')
    print(f'average number of customers in system: {statistics.average_customers_in_system}')
    print(f'average time in queue: {statistics.average_time_in_queue}')
    print(f'average time in system: {statistics.average_time_in_system}')
    print(f'average busy servers: {statistics.average_busy_servers}')
    print()


def print_plots(parameters: QueueingSystemParameters, theoretical: TheoreticalStatistics, args: argparse.Namespace) -> None:
    import matplotlib.pyplot as plt
    import copy
    from matplotlib.figure import Figure
    from matplotlib.axes._subplots import Axes

    data = copy.deepcopy(parameters)
    p_count = data.server_count + data.queue_capacity + 1  # total probabilities number

    # init plot objects
    fig: Figure
    axes: ndarray
    fig, axes = plt.subplots(ncols=1, nrows=p_count, figsize=(16, 28), gridspec_kw={'height_ratios': [1] * p_count, 'width_ratios': [1]})
    plt.xlabel(f'lambda = {data.arrival_rate}, mu = {data.service_rate}, n = {data.server_count}, m = {data.queue_capacity}, v = {data.v}')

    # generate queueing systems and save probabilities of each system
    times = [float(t) for t in range(args.start, args.end, args.step)]
    probabilities: list[list[float]] = [[] for _ in range(p_count)]
    for t in times:
        data.duration = t
        s = QueueingSystem(data, simpy.Environment(0)).run()
        for i, p in enumerate(s.probabilities):
            probabilities[i].append(p)

    # set plot for every probability
    colors: list[str] = plt.rcParams['axes.prop_cycle'].by_key()['color']
    c_count = len(colors)
    for i in range(p_count):
        a: Axes = axes[i]  # actual type is matplotlib.axes._subplots.AxesSubplot
        color = colors[i % c_count]
        a.set_title(f'p{i}')
        a.fill_between(times, y1=probabilities[i], step='post', alpha=0.5, color=color)
        a.plot(times, [theoretical.probabilities[i]] * len(times), '-k')

    # show plots
    fig.tight_layout()
    fig.show()

    # save plots to disk (useful when running via terminal)
    from datetime import datetime
    now = datetime.now()
    now_str = now.strftime('%d-%m-%Y_%H-%M')
    fig.savefig(now_str + '_plots')


def _main():
    args = get_args(setup_parser())
    parameters = get_input_parameters(args)
    print_input(parameters)
    ts = TheoreticalStatistics(parameters)
    print_statistics(ts)
    system = QueueingSystem(parameters, simpy.Environment(0)).run()
    print_statistics(system, theoretical=False)
    print_plots(parameters, ts, args)


if __name__ == '__main__':
    _main()
