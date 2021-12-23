![logo](robosharp.png?raw=true)
# RoboSharp
| <h2>[RoboSharp Going Forward...](https://github.com/tjscience/RoboSharp/issues/63)</h2> | [![image](https://user-images.githubusercontent.com/3706870/44311401-a9064000-a3b4-11e8-96a3-d308f52aeec1.png)](https://github.com/tjscience/RoboSharp/issues/63) |
| ------ | ----------- |
### Now available on nuget! https://www.nuget.org/packages/RoboSharp/
RoboSharp is a .NET wrapper for the awesome Robocopy windows application.

Robocopy is a very extensive file copy application written by microsoft and included in modern versions of Windows. To learn more about Robocopy, visit the documentation page at http://technet.microsoft.com/en-us/library/cc733145.aspx.

RoboSharp came out of a need to manipulate Robocopy in a c# backup application that I was writing. It has helped me tremendously so I thought that I would share it! It exposes all of the switches available in RoboCopy as descriptive properties. With RoboSharp, you can subscribe to events that fire when files are processed, errors occur and even as the progress of a file copy changes. Another really nice feature of RoboSharp is that you can pause and resume a copy that is in progress which is a feature that I though was lacking in Robocopy.

In the project, you will find the RoboSharp library as well as a sample backup application that shows off many (but not all) of the options. If you like the project, please rate it!

Here is an example of how you would use RoboSharp:

```c#
public void Backup()
{
    RoboCommand backup = new RoboCommand();
    // events
    backup.OnFileProcessed += backup_OnFileProcessed;
    backup.OnCommandCompleted += backup_OnCommandCompleted;
    // copy options
    backup.CopyOptions.Source = Source.Text;
    backup.CopyOptions.Destination = Destination.Text;
    backup.CopyOptions.CopySubdirectories = true;
    backup.CopyOptions.UseUnbufferedIo = true;            
    // select options
    backup.SelectionOptions.OnlyCopyArchiveFilesAndResetArchiveFlag = true;
    // retry options
    backup.RetryOptions.RetryCount = 1;
    backup.RetryOptions.RetryWaitTime = 2;
    backup.Start();
}

void backup_OnFileProcessed(object sender, FileProcessedEventArgs e)
{
    Dispatcher.BeginInvoke((Action)(() =>
    {
        CurrentOperation.Text = e.ProcessedFile.FileClass;
        CurrentFile.Text = e.ProcessedFile.Name;
        CurrentSize.Text = e.ProcessedFile.Size.ToString();
    }));
}

void backup_OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e)
{
    Dispatcher.BeginInvoke((Action)(() =>
    {
        MessageBox.Show("Backup Complete!");
    }));
}
```

#### Extended Results:

See below examples on how to access the extended results where you can get total, copied, skipped, mismatch, failed and extra statistics for directories and files and bytes, as well as additional speed info e.g. bytes per sec and megabytes per minute

```c#
void copy_OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                OptionsGrid.IsEnabled = true;
                ProgressGrid.IsEnabled = false;

                var results = e.Results;
                Console.WriteLine("Files copied: " + results.FilesStatistic.Copied);
                Console.WriteLine("Directories copied: " + results.DirectoriesStatistic.Copied);
                Console.WriteLine("MegaBytesPerMin: " + results.SpeedStatistic.MegaBytesPerMin);
            }));
        }
```

or

```c#
var cmd = new RoboCommand();
var results = await cmd.StartAsync();
// do something with results...
```

or

```c#
var cmd = new RoboCommand();
var copyTask = cmd.Start();
copyTask.Wait(); // be careful to avoid deadlocks here!
var results = cmd.GetResults();
// do something with results...
```

or

```c#
var cmd = new RoboCommand();
cmd.OnCommandCompleted += (args) => 
{
    var results = args.Results;
    // do something with results...
}
cmd.Start();
```

.AddStatistic

This is useful if you are running multiple RoboCopy tasks as it allows you to add all the statistics to each other to generating overall results

```c#
RoboSharp.Results.Statistic FileStats = new RoboSharp.Results.Statistic();
RoboSharp.Results.Statistic DirStats = new RoboSharp.Results.Statistic();

test = new RoboCommand();

// Run first task and add results
test.CopyOptions.Source = @"C:\SOURCE_1";
test.CopyOptions.Destination = @"C:\DESTINATION";

RoboSharp.Results.RoboCopyResults results1 = await test.StartAsync();

FileStats.AddStatistic(results1.FilesStatistic);
DirStats.AddStatistic(results1.DirectoriesStatistic);

// Run second task and add results
test.CopyOptions.Source = @"C:\SOURCE_2";
test.CopyOptions.Destination = @"C:\DESTINATION";

RoboSharp.Results.RoboCopyResults results2 = await test.StartAsync();

FileStats.AddStatistic(results2.FilesStatistic);
DirStats.AddStatistic(results2.DirectoriesStatistic);
```

You could also use .AddStatistic in the OnCommandCompleted event e.g.

```c#
void copy_OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e)
        {
            this.BeginInvoke((Action)(() =>
            {
                // Get robocopy results 
                RoboSharp.Results.RoboCopyResults AnalysisResults = e.Results;

                FileStats.AddStatistic(AnalysisResults.FilesStatistic);
            }));
        }
```


N.B. The below has been superseded by changes in PR #127 - documentation will be updated shortly to cover all new methods

.AverageStatistics

Again if running multiple RoboCopy tasks you can use this to get average results for BytesPerSec and MegaBytesPerMin 

Based on above example

```c#
// Call Static Method to return new object with the average
RoboSharp.Results.SpeedStatistic avg = RoboSharp.Results.SpeedStatistic.AverageStatistics(new RoboSharp.Results.SpeedStatistic[] { results1.SpeedStatistic, results2.SpeedStatistic });
```

or

```c#
// Result1 will now store the average statistic value. Result 2 can be disposed or or re-used for additional RoboCopy commands.
results1.AverageStatistic(results2);
```

=======

# Contributing to RoboSharp

First off, thanks! Please go through the [guidelines](CONTRIBUTING.md).