using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FYP_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewAttendancePage : ContentPage
    {
        public ApiService ApiService { get; set; }
        //public List<StudentAttendanceGraph> StudentAttendanceGraph { get; set; } = new List<StudentAttendanceGraph>();
        public ViewAttendancePage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            //StudentAttendanceGraph = studentAttendanceGraphs;

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            dateIcon.Source = ImageSource.FromResource("FYP_App.Image.Date icon.png");

            todayDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy");

            GenerateTimeGrid();

            displayInfo();
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UserMainPage());
        }

        private void TapDetailButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new ViewAttendanceDetailPage());
        }

        private void GenerateTimeGrid()
        {
            int startHour = 8;
            int endHour = 18;
            int totalHours = endHour - startHour + 1;

            // every hour 2 row 
            int totalRows = totalHours * 2;

            for (int i = 0; i < totalRows; i++)
            {
                TimeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) }); // 每行高度
            }

            int currentRow = 0;

            for (int hour = startHour; hour <= endHour; hour++)
            {
                timeSlot.Add(hour);

                var timeLabel = new Label
                {
                    Text = $"{hour:00}:00",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromHex("#333"),
                    FontSize = 16
                };

                TimeGrid.Children.Add(timeLabel, 0, currentRow);
                Grid.SetRowSpan(timeLabel, 2);

                // right side content
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0) // only add line at first row for each hour 
                    {
                        var borderLine = new BoxView
                        {
                            HeightRequest = 1,
                            BackgroundColor = Color.Black,
                            VerticalOptions = LayoutOptions.End,
                            HorizontalOptions = LayoutOptions.FillAndExpand
                        };

                        TimeGrid.Children.Add(borderLine, 1, currentRow + i);
                    }
                }

                currentRow += 2;
                //System.Diagnostics.Debug.WriteLine($"Added hour: {hour}");
            }
        }

        public List<int> timeSlot = new List<int>();

        async void displayInfo()
        {
            var res = await ApiService.RetrieveStudentTodayAttendance(App.User.MatricNo);

            if (res != null)
            {
                foreach (var attendance in res)
                {
                    var startindex = timeSlot.FindIndex(x => x == attendance.StartTime);
                    Console.WriteLine("Start index: " + startindex);

                    var endindex = timeSlot.FindIndex(x => x == attendance.EndTime);
                    Console.WriteLine("End index: " + endindex);

                    var numRow = (endindex - startindex) * 2;

                    bool isValid = attendance.IsValid == 1 ? true : false;

                    var label = "";
                    if (isValid)
                    {
                        label = attendance.SubjectId + '(' + attendance.SessionType + ')'
                            + '\n' + attendance.SubjectName;
                    }
                    else
                    {
                        label = attendance.SubjectId + '(' + attendance.SessionType + ')'
                            + '\n' + attendance.SubjectName
                            + '\n' + "Note: Location is invalid (determine by lecturer)";
                    }

                    MergeColumnRows(numRow, startindex * 2, label, isValid); 
                    // start index * 2 because each time is occupied 2 row 
                }
            }
        }

        void MergeColumnRows(int numMergeRow, int startRow, string labelText, bool isValid)
        {
            startRow += 1; // this is because first row is the line, not space between time
            var framesToRemove = TimeGrid.Children
                .Where(c => Grid.GetColumn(c) == 1 && Grid.GetRow(c) >= startRow && Grid.GetRow(c) <= (startRow + numMergeRow))
                .ToList(); 

            foreach (var f in framesToRemove)
            {
                TimeGrid.Children.Remove(f);
            }

            var mergedGrid = new Grid();
            mergedGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            //mergedGrid.Padding = new Thickness(0, 0, 0, 10);
            //mergedGrid.RowDefinitions.Add(new RowDefinition { Height = 10 });

            //var contentGrid = new Grid();

            var labelFrame = new Frame
            {
                CornerRadius = 8,
                BackgroundColor = isValid ? Color.FromHex("#37D13F") : Color.FromHex("#FF0E00"),
                HasShadow = false,
                Padding = new Thickness(20),
                Margin = new Thickness(5, 0, 5, 5),
                //HeightRequest = 50,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                Content = new Label
                {
                    Text = labelText,
                    TextColor = Color.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 18
                }
            };
            mergedGrid.Children.Add(labelFrame);

            var bottomLine = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Color.Black,
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            mergedGrid.Children.Add(bottomLine);

            //mergedGrid.Children.Add(contentGrid, 0, 0);

            var mergedFrame = new Frame
            {
                CornerRadius = 8,
                BackgroundColor = Color.Transparent,
                HasShadow = false,
                Padding = new Thickness(8, 0),
                Content = mergedGrid
            };

            TimeGrid.Children.Add(mergedFrame, 1, startRow);
            Grid.SetRowSpan(mergedFrame, numMergeRow);
        }
    }
}