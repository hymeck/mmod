from des import *
from prettytable import PrettyTable


class SpecialQueueingSystem(QueueingSystem):
    def _handle_request(self, request): return request  # ignore patience of base class


class SpecialTheoreticalStatistics(TheoreticalStatistics):
    def _p0(self) -> float: return (1 - self.rho) / (1 - (self.rho ** (self.m + 2)))

    @property
    def probabilities(self) -> list[float]:
        if self._probabilities is None:
            p0 = self._p0()
            pk = [p0 * (self.rho ** k) for k in range(1, self.m + 2)]
            self._probabilities = [p0] + pk
        return self._probabilities

    @property
    def average_customers_in_queue(self) -> float:
        rho, m, p0 = self.rho, self.m, self.probabilities[0]
        return rho * rho * ((1 - (rho ** m) * (m * (1 - rho) + 1)) / ((1 - rho) ** 2)) * p0

    @property
    def average_customers_in_system(self) -> float: return 1. + self.average_customers_in_queue


def _main():
    from copy import deepcopy

    args = get_args(setup_parser())
    parameters1 = get_input_parameters(args)
    parameters1.arrival_rate = 4.
    parameters1.service_rate = 2.
    parameters1.server_count = 1

    service_rates = [2., 5.]
    for service_rate in service_rates:
        parameters1.service_rate = service_rate
        parameters1.queue_capacity = 3
        print_input(parameters1)

        ts1 = SpecialTheoreticalStatistics(parameters1)
        print('with q = 3')
        print_statistics(statistics=ts1)
        system1 = SpecialQueueingSystem(parameters1, simpy.Environment(0)).run()
        print_statistics(statistics=system1, theoretical=False)

        parameters2 = deepcopy(parameters1)
        parameters2.queue_capacity = 4

        ts2 = SpecialTheoreticalStatistics(parameters2)
        print('with q = 4')
        print_statistics(statistics=ts2)
        system2 = SpecialQueueingSystem(parameters2, simpy.Environment(0)).run()
        print_statistics(statistics=system2, theoretical=False)

        statistics_table = PrettyTable(['statistics', 'q = 3', 'q = 4'])
        statistics_table.add_row(['rejection probability', system1.rejection_probability, system2.rejection_probability])
        statistics_table.add_row(['relative bandwidth', system1.relative_bandwidth, system2.relative_bandwidth])
        statistics_table.add_row(['absolute bandwidth', system1.absolute_bandwidth, system2.absolute_bandwidth])
        statistics_table.add_row(['average customers in queue', system1.average_customers_in_queue, system2.average_customers_in_queue])
        statistics_table.add_row(['average customers in system', system1.average_customers_in_system, system2.average_customers_in_system])
        statistics_table.add_row(['average time in queue', system1.average_time_in_queue, system2.average_time_in_queue])
        statistics_table.add_row(['average time in system', system1.average_time_in_system, system2.average_time_in_system])
        statistics_table.add_row(['average busy servers', system1.average_busy_servers, system2.average_busy_servers])
        print(statistics_table)
        print_chi_squared(ts=ts1, qs=system1)
        print_chi_squared(ts=ts2, qs=system2)
        print('------------')
        print()


if __name__ == '__main__':
    _main()
