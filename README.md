
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

# Contributing to RoboSharp

First off, thanks! Please go through the [guidelines](CONTRIBUTING.md).
