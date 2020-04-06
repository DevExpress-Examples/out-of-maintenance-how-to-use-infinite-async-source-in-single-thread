using DevExpress.Data.Filtering;
using DevExpress.Xpf.Data;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InfiniteAsyncSourceSingleThreadSample {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            CreateSource();
        }
        async void CreateSource() {
            var scheduler = new SingleThreadTaskScheduler();
            var factory = new TaskFactory(scheduler);

            var source = new InfiniteAsyncSource() {
                ElementType = typeof(IssueData)
            };
            Closed += (o, e) => {
                source.Dispose();
                scheduler.Complete();
            };

            var issuesContext = await factory.StartNew(() => new IssuesContext());

            source.FetchRows += (o, e) => {
                e.Result = factory.StartNew(() => FetchRows(issuesContext, e));
            };

            source.GetUniqueValues += (o, e) => {
                if(e.PropertyName == "Priority") {
                    var values = Enum.GetValues(typeof(Priority)).Cast<object>().ToArray();
                    e.Result = Task.FromResult(values);
                } else {
                    throw new InvalidOperationException();
                }
            };

            source.GetTotalSummaries += (o, e) => {
                e.Result = factory.StartNew(() => GetTotalSummaries(issuesContext, e));
            };

            grid.ItemsSource = source;
        }

        static FetchRowsResult FetchRows(IssuesContext issuesContext, FetchRowsAsyncEventArgs e) {
            IssueSortOrder sortOrder = GetIssueSortOrder(e);
            IssueFilter filter = MakeIssueFilter(e.Filter);

            var take = e.Take ?? 30;
            var issues = issuesContext.GetIssues(
                skip: e.Skip,
                take: take,
                sortOrder: sortOrder,
                filter: filter);

            return new FetchRowsResult(issues, hasMoreRows: issues.Length == take);
        }

        static object[] GetTotalSummaries(IssuesContext issuesContext, GetSummariesAsyncEventArgs e) {
            IssueFilter filter = MakeIssueFilter(e.Filter);
            var summaryValues = issuesContext.GetSummaries(filter);
            return e.Summaries.Select(x => {
                if(x.SummaryType == SummaryType.Count)
                    return (object)summaryValues.Count;
                if(x.SummaryType == SummaryType.Max && x.PropertyName == "Created")
                    return summaryValues.LastCreated;
                throw new InvalidOperationException();
            }).ToArray();
        }

        static IssueSortOrder GetIssueSortOrder(FetchRowsAsyncEventArgs e) {
            if(e.SortOrder.Length > 0) {
                var sort = e.SortOrder.Single();
                if(sort.PropertyName == "Created") {
                    if(sort.Direction != ListSortDirection.Descending)
                        throw new InvalidOperationException();
                    return IssueSortOrder.CreatedDescending;
                }
                if(sort.PropertyName == "Votes") {
                    return sort.Direction == ListSortDirection.Ascending
                        ? IssueSortOrder.VotesAscending
                        : IssueSortOrder.VotesDescending;
                }
            }
            return IssueSortOrder.Default;
        }

        static IssueFilter MakeIssueFilter(CriteriaOperator filter) {
            return filter.Match(
                binary: (propertyName, value, type) => {
                    if(propertyName == "Votes" && type == BinaryOperatorType.GreaterOrEqual)
                        return new IssueFilter(minVotes: (int)value);

                    if(propertyName == "Priority" && type == BinaryOperatorType.Equal)
                        return new IssueFilter(priority: (Priority)value);

                    if(propertyName == "Created") {
                        if(type == BinaryOperatorType.GreaterOrEqual)
                            return new IssueFilter(createdFrom: (DateTime)value);
                        if(type == BinaryOperatorType.Less)
                            return new IssueFilter(createdTo: (DateTime)value);
                    }

                    throw new InvalidOperationException();
                },
                and: filters => {
                    return new IssueFilter(
                        createdFrom: filters.Select(x => x.CreatedFrom).SingleOrDefault(x => x != null),
                        createdTo: filters.Select(x => x.CreatedTo).SingleOrDefault(x => x != null),
                        minVotes: filters.Select(x => x.MinVotes).SingleOrDefault(x => x != null),
                        priority: filters.Select(x => x.Priority).SingleOrDefault(x => x != null)
                    );
                },
                @null: default(IssueFilter)
            );
        }
    }
}
