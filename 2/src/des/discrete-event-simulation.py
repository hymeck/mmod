import argparse
import sys
import simpy
import numpy as np


class InputDataHolder:
    def __init__(self, arrival_rate: float, service_rate: float, server_count: int, queue_capacity: int,
                 waiting_time: float, duration: float):
        self.arrival_rate = arrival_rate
        self.service_rate = service_rate
        self.server_count = server_count
        self.queue_capacity = queue_capacity
        self.waiting_time = waiting_time
        self.duration = duration


class StatisticsHolder:
    def __init__(self):
        self.times_in_system = []
        self.times_in_queue = []
        self.customer_count_in_system = []
        self.customer_count_in_queue = []
        self.served_customers = []
        self.rejected_customer_count = 0


class QueueingSystem:
    def __init__(self, holder: InputDataHolder, env: simpy.Environment):
        self.holder = holder
        self.env = env
        self.server = simpy.Resource(env, holder.server_count)
        self.statistics = StatisticsHolder()
        self._probabilities = None
        self._average_customers_in_queue = None
        self._average_customers_in_system = None
        self._average_time_in_queue = None
        self._average_time_in_system = None
        self._average_busy = None

    def _simulate_customer_arrival(self):
        return self.env.timeout(np.random.exponential(1 / self.holder.arrival_rate))

    def _get_customer_patience_generator(self):
        yield self.env.timeout(np.random.exponential(self.holder.waiting_time))

    def _get_server_processing_generator(self):
        yield self.env.timeout(np.random.exponential(1 / self.holder.service_rate))

    @property
    def _waiting_customers(self) -> int:
        return len(self.server.queue)

    @property
    def _served_customers(self) -> int:
        return self.server.count

    @property
    def probabilities(self) -> list[float]:
        if self._probabilities is None:
            self._probabilities = self._get_probabilities()
        return self._probabilities

    @property
    def rejection_probability(self) -> float:
        return self.probabilities[-1]

    @property
    def relative_bandwidth(self) -> float:
        return 1 - self.rejection_probability

    @property
    def absolute_bandwidth(self) -> float:
        return self.holder.arrival_rate * self.relative_bandwidth

    @property
    def average_customers_in_queue(self) -> float:
        if self._average_customers_in_queue is None:
            self._average_customers_in_queue = np.array(self.statistics.customer_count_in_queue).mean()
        return self._average_customers_in_queue

    @property
    def average_customers_in_system(self) -> float:
        if self._average_customers_in_system is None:
            self._average_customers_in_system = np.array(self.statistics.customer_count_in_system).mean()
        return self._average_customers_in_system

    @property
    def average_time_in_queue(self) -> float:
        if self._average_time_in_queue is None:
            self._average_time_in_queue = np.array(self.statistics.times_in_queue).mean()
        return self._average_time_in_queue

    @property
    def average_time_in_system(self) -> float:
        if self._average_time_in_system is None:
            self._average_time_in_system = np.array(self.statistics.times_in_system).mean()
        return self._average_time_in_system

    @property
    def average_busy_servers(self) -> float:
        if self._average_busy is None:
            self._average_busy = self.relative_bandwidth * self.holder.arrival_rate / self.holder.service_rate
        return self._average_busy


    def _serve_customer(self):
        in_queue_before_request = self._waiting_customers
        in_server_before_request = self._served_customers

        self.statistics.customer_count_in_queue.append(in_queue_before_request)
        self.statistics.customer_count_in_system.append(in_queue_before_request + in_server_before_request)

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
                self.statistics.rejected_customer_count = self.statistics.rejected_customer_count + 1
                self.statistics.times_in_queue.append(0)
                self.statistics.times_in_system.append(0)

    def _generate_customers(self):
        while True:
            self.env.process(self._serve_customer())
            yield self._simulate_customer_arrival()

    def run(self) -> None:
        self.env.process(self._generate_customers())
        self.env.run(self.holder.duration)

    def _get_probabilities(self) -> list[float]:
        n = self.holder.server_count
        m = self.holder.queue_capacity
        rejected = self.statistics.rejected_customer_count
        total_count = len(self.statistics.served_customers) + rejected
        last = rejected / total_count
        return [self.statistics.served_customers.count(i) / total_count for i in range(1, n + m + 1)] + [last]

# region + args processing +

def setup_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser()
    parser.add_argument('duration', help='time in hours to simulate system', type=float)
    parser.add_argument('-ar', '--arrival-rate', help='arrival rate mean', type=float)
    parser.add_argument('-sr', '--service-rate', help='service rate mean', type=float)
    parser.add_argument('-sc', '--server-count', help='number of servers in a system', type=int)
    parser.add_argument('-qc', '--queue-capacity', help='capacity of a queue', type=int)
    parser.add_argument('-wt', '--waiting-time', help='mean of waiting time', type=float)
    return parser


def get_args(parser: argparse.ArgumentParser) -> argparse.Namespace:
    try:
        args = parser.parse_args()
        return args
    except:
        return sys.exit(1)


# endregion + args processing +


# region + parameter fetching +

def _get_value_or_default(args: argparse.Namespace, parameter_name: str, default_value: any) -> any:
    value = args.__dict__.get(parameter_name)
    return value if value is not None else default_value


def get_arrival_rate(args: argparse.Namespace) -> float:
    return _get_value_or_default(args, 'arrival_rate', float(10))


def get_service_rate(args: argparse.Namespace) -> float:
    return _get_value_or_default(args, 'service_rate', float(3))


def get_server_count(args: argparse.Namespace) -> int:
    return _get_value_or_default(args, 'server_count', 3)


def get_queue_capacity(args: argparse.Namespace) -> int:
    return _get_value_or_default(args, 'queue_capacity', 4)


def get_waiting_time(args: argparse.Namespace) -> float:
    return _get_value_or_default(args, 'queue_capacity', 0.166666667)


def get_duration(args: argparse.Namespace) -> float:
    return args.duration


# endregion + parameter fetching +


# region + printing +

def print_input(input_holder: InputDataHolder) -> None:
    print(f'Arrival rate: {input_holder.arrival_rate}')
    print(f'Service rate: {input_holder.service_rate}')
    print(f'Server count: {input_holder.server_count}')
    print(f'Queue capacity: {input_holder.queue_capacity}')
    print(f'Waiting time: {input_holder.waiting_time}')
    print(f'Investigation duration: {input_holder.duration}')


# endregion + printing +

def get_input_holder(args: argparse.Namespace) -> InputDataHolder:
    return InputDataHolder(get_arrival_rate(args), get_service_rate(args), get_server_count(args),
                           get_queue_capacity(args),
                           get_waiting_time(args), args.duration)


def main():
    args = get_args(setup_parser())
    input_holder = get_input_holder(args)
    print_input(input_holder)
    system = QueueingSystem(input_holder, simpy.Environment(0))
    system.run()
    print(f'Probabilities:')
    for i, p in enumerate(system.probabilities):
        print(f'#{i}: {p}')
    print(f'Sum: {sum(system.probabilities)}')
    print(f'Probability of rejection: {system.rejection_probability}')
    print(f'Relative bandwidth: {system.relative_bandwidth}')
    print(f'Absolute bandwidth: {system.absolute_bandwidth}')
    print(f'Average number of customers in queue: {system.average_customers_in_queue}')
    print(f'Average number of customers in system: {system.average_customers_in_system}')
    print(f'Average time in queue: {system.average_time_in_queue}')
    print(f'Average time in system: {system.average_time_in_system}')
    print(f'Average busy servers: {system.average_busy_servers}')


if __name__ == '__main__':
    main()
