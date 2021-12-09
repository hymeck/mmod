import datetime

import matplotlib.figure
import matplotlib.pyplot as plt
import numpy as np

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


def plot_statistic_comparison(emp_statistic1, emp_statistic2, label, parameters) -> matplotlib.figure.Figure:
    fig: matplotlib.figure.Figure
    fig, ax = plt.subplots()
    data = [emp_statistic1, emp_statistic2]
    x = np.arange(2)
    # ax.bar(x + 0.00, data[0], color='y', width=0.25, alpha=0.5, label='with q=3')
    # ax.bar(x + 0.00, data[1], color='r', width=0.25, alpha=0.5, label='with q=4')
    ax.bar([0 - 0.1], [data[0]], color='y', width=0.2, alpha=0.5, label='with q=3')
    ax.bar([0 + 0.1], [data[1]], color='r', width=0.2, alpha=0.5, label='with q=4')
    plt.title(label)
    plot_label = f'lambda = {parameters.arrival_rate}, mu = {parameters.service_rate}, n = {parameters.server_count}'
    plt.xlabel(plot_label)
    fig.legend()
    return fig


def plot_statistics_comparison(qs1: SpecialQueueingSystem, qs2: SpecialQueueingSystem, args: argparse.Namespace, params: QueueingSystemParameters) -> None:
    datetime_format = '%Y-%m-%d_%H-%M-%S-%f'
    data = [
        (qs1.average_customers_in_queue, qs2.average_customers_in_queue, 'average customers in queue'),
        (qs1.average_customers_in_system, qs2.average_customers_in_system, 'average customers in system'),
        (qs1.average_time_in_queue, qs2.average_time_in_queue, 'average time in queue'),
        (qs1.average_time_in_system, qs2.average_time_in_system, 'average time in system')
    ]
    for d in data:
        f = plot_statistic_comparison(d[0], d[1], d[2], params)
        if args.no_plot is not True:
            plt.show()
        if args.save_plot is True:
            now = datetime.datetime.now().strftime(datetime_format)
            f.savefig(now + '_' + d[2].replace(' ', '_'))
        f.clf()
    pass


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
        print_plots(parameters1, system1, ts1.probabilities, args)

        parameters2 = deepcopy(parameters1)
        parameters2.queue_capacity = 4

        ts2 = SpecialTheoreticalStatistics(parameters2)
        print('with q = 4')
        print_statistics(statistics=ts2)
        system2 = SpecialQueueingSystem(parameters2, simpy.Environment(0)).run()
        print_statistics(statistics=system2, theoretical=False)
        print_plots(parameters2, system2, ts2.probabilities, args)

        plot_statistics_comparison(system1, system2, args, parameters1)

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
