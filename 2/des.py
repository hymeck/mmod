import argparse
import math
import sys
import simpy
from numpy.random import exponential as exp
from functools import reduce
from typing import Optional, Union
import numpy as np
import matplotlib.pyplot as plt


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

    def _handle_request(self, request): return request | self.env.process(self._get_customer_patience_generator())

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
                yield self._handle_request(request)
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
    # def average_busy_servers(self) -> float: return self.relative_bandwidth * self.holder.server_count
    def average_busy_servers(self) -> float: return self.relative_bandwidth * self.holder.arrival_rate / self.holder.service_rate

    def run(self):
        self.env.process(self._generate_customers())
        self.env.run(until=self.holder.duration)
        return self


def setup_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser()
    parser.add_argument('-d', '--duration', help='simulation time (used in simpy.Environment.run())', type=int, default=5000)
    parser.add_argument('-ar', '--arrival-rate', help='arrival rate mean', type=float, default=10.)
    parser.add_argument('-sr', '--service-rate', help='service rate mean', type=float, default=3.)
    parser.add_argument('-sc', '--server-count', help='number of servers in a system', type=int, default=3)
    parser.add_argument('-qc', '--queue-capacity', help='capacity of a queue', type=int, default=4)
    parser.add_argument('-v', '--v', help='v', type=float, default=6.)
    parser.add_argument('-ic', '--interval-count', help='count of intervals for plot', type=int, default=100)
    parser.add_argument('-id', '--investigation-duration', help='how many system will run for plot', type=int, default=10000)
    parser.add_argument('-np', '--no-plot', help='flag for disabling plt.show()', action='store_true', default=False)
    parser.add_argument('-sp', '--save-plot', help='flag for saving plots to disk', action='store_true', default=False)
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
    # def average_busy_servers(self) -> float: return self.relative_bandwidth * self.parameters.server_count
    def average_busy_servers(self) -> float: return self.relative_bandwidth * self.rho


def print_statistics(statistics: Union[QueueingSystem, TheoreticalStatistics], theoretical: bool = True) -> None:
    header = 'theoretical' if theoretical else 'empirical'
    print(f'{header} statistics')
    print(f'probabilities:')
    for i, p in enumerate(statistics.probabilities):
        print(f'#{i}: {p}')
    print(f'sum: {sum(statistics.probabilities)}')
    print()
    print(f'probability of rejection: {statistics.rejection_probability}')
    print(f'relative bandwidth: {statistics.relative_bandwidth}')
    print(f'absolute bandwidth: {statistics.absolute_bandwidth}')
    print(f'average number of customers in queue: {statistics.average_customers_in_queue}')
    print(f'average number of customers in system: {statistics.average_customers_in_system}')
    print(f'average time in queue: {statistics.average_time_in_queue}')
    print(f'average time in system: {statistics.average_time_in_system}')
    print(f'average busy servers: {statistics.average_busy_servers}')
    print()


def print_plots(parameters: QueueingSystemParameters, s: QueueingSystem, theoretical_probabilities: list[float], args: argparse.Namespace) -> None:
    import copy
    from datetime import datetime

    data = copy.deepcopy(parameters)
    data.duration = args.investigation_duration
    interval_count = args.interval_count

    intervals = np.array_split(s.statistics.customers_in_system, interval_count)
    for i in range(1, len(intervals)):
        intervals[i] = np.append(intervals[i], intervals[i - 1])

    # print final probabilities bars
    fig, axs = plt.subplots()
    axs.bar([i - 0.1 for i in range(len(s.probabilities))], s.probabilities, width=0.2, alpha=0.5, label='empirical')
    axs.bar([i + 0.1 for i in range(len(theoretical_probabilities))], theoretical_probabilities, width=0.2, alpha=0.5, label='theoretical')
    plt.title('final probabilities')

    # for lab2
    # plot_label = f'lambda = {data.arrival_rate}, mu = {data.service_rate}, n = {data.server_count}, m = {data.queue_capacity}, v = {data.v}'
    # for lab3
    plot_label = f'lambda = {data.arrival_rate}, mu = {data.service_rate}, n = {data.server_count}, m = {data.queue_capacity}'

    plt.xlabel(plot_label)
    plt.legend()

    datetime_format = '%Y-%m-%d_%H-%M-%S-%f'
    if args.no_plot is not True:
        plt.show()
    if args.save_plot is True:
        now = datetime.now()
        now_str = now.strftime(datetime_format)
        plt.savefig(now_str + '_final_probs')

    # print histogram
    for i in range(len(theoretical_probabilities)):
        interval_probabilities = []
        for interval in intervals:
            interval_probabilities.append(len(interval[interval == i]) / len(interval))

        plt.figure(figsize=(5, 5))
        plt.bar(range(len(interval_probabilities)), interval_probabilities)
        plt.title(f'probability {i}')
        plt.axhline(y=theoretical_probabilities[i], xmin=0, xmax=len(interval_probabilities), color='red')
        plt.xlabel(plot_label)
        if args.no_plot is not True:
            plt.show()
        if args.save_plot is True:
            now = datetime.now()
            now_str = now.strftime(datetime_format)
            plt.savefig(now_str + '_plots_prob' + str(i))
        plt.clf()


def print_chi_squared(ts: TheoreticalStatistics, qs: QueueingSystem) -> None:
    from scipy.stats import chi2_contingency
    chi2, p, dof, ex = chi2_contingency(np.array([ts.probabilities, qs.probabilities]))
    print(f'chi-squared: {p}')


def run_system(parameters: QueueingSystemParameters, args: argparse.Namespace) -> None:
    print_input(parameters)
    ts = TheoreticalStatistics(parameters)
    print_statistics(ts)
    print()
    system = QueueingSystem(parameters, simpy.Environment(0)).run()
    print_statistics(system, theoretical=False)
    print()
    print_chi_squared(ts, system)
    print_plots(parameters, system, ts.probabilities, args)


def _main():
    args = get_args(setup_parser())
    parameters = get_input_parameters(args)
    run_system(parameters, args)
    print('-------------------')

    parameters.service_rate = parameters.service_rate + 1.
    run_system(parameters, args)
    print('-------------------')

    parameters.service_rate = parameters.service_rate + 2.
    run_system(parameters, args)


if __name__ == '__main__':
    _main()
