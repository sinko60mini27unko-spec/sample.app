using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ScheduleTasksApp;

public partial class MainWindow : Window
{
    private readonly DatabaseService _databaseService = new();
    private DateItem? _selectedDate;
    private TaskItem? _selectedTask;

    public MainWindow()
    {
        InitializeComponent();
        _databaseService.Initialize();
        LoadDates();
    }

    private void LoadDates()
    {
        var dates = _databaseService.GetDates();
        DateListBox.ItemsSource = dates;

        if (dates.Count > 0)
        {
            DateListBox.SelectedIndex = 0;
        }
        else
        {
            SelectedDateText.Text = "日付を選択してください";
            TasksDataGrid.ItemsSource = new List<TaskItem>();
        }
    }

    private void LoadTasks(int dateId)
    {
        var tasks = _databaseService.GetTasksByDate(dateId);
        TasksDataGrid.ItemsSource = tasks;
        ClearTaskEditor();
    }

    private void DateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DateListBox.SelectedItem is not DateItem dateItem)
        {
            _selectedDate = null;
            return;
        }

        _selectedDate = dateItem;
        SelectedDateText.Text = $"{dateItem.DisplayText} のタスク";
        LoadTasks(dateItem.Id);
    }

    private void TasksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TasksDataGrid.SelectedItem is not TaskItem task)
        {
            _selectedTask = null;
            return;
        }

        _selectedTask = task;
        TitleTextBox.Text = task.Title;
        AssigneeTextBox.Text = task.Assignee;
        NoteTextBox.Text = task.Note;
        IsDoneCheckBox.IsChecked = task.IsDone;
    }

    private void AddTodayDate_Click(object sender, RoutedEventArgs e)
    {
        _databaseService.UpsertDate(DateOnly.FromDateTime(DateTime.Today));
        LoadDates();
    }

    private void DeleteDate_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDate is null)
        {
            MessageBox.Show("削除する日付を選択してください。");
            return;
        }

        var result = MessageBox.Show("この日付と関連タスクを削除します。よろしいですか？", "確認", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _databaseService.DeleteDate(_selectedDate.Id);
        LoadDates();
    }

    private void AddTask_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDate is null)
        {
            MessageBox.Show("先に日付を選択してください。");
            return;
        }

        var task = BuildTaskFromInputs();
        if (task is null)
        {
            return;
        }

        task.DateId = _selectedDate.Id;
        _databaseService.AddTask(task);
        LoadTasks(_selectedDate.Id);
    }

    private void UpdateTask_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDate is null || _selectedTask is null)
        {
            MessageBox.Show("更新するタスクを選択してください。");
            return;
        }

        var task = BuildTaskFromInputs();
        if (task is null)
        {
            return;
        }

        task.Id = _selectedTask.Id;
        task.DateId = _selectedDate.Id;
        _databaseService.UpdateTask(task);
        LoadTasks(_selectedDate.Id);
    }

    private void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDate is null || _selectedTask is null)
        {
            MessageBox.Show("削除するタスクを選択してください。");
            return;
        }

        _databaseService.DeleteTask(_selectedTask.Id);
        LoadTasks(_selectedDate.Id);
    }

    private TaskItem? BuildTaskFromInputs()
    {
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            MessageBox.Show("タイトルを入力してください。");
            return null;
        }

        return new TaskItem
        {
            Title = TitleTextBox.Text.Trim(),
            Assignee = AssigneeTextBox.Text.Trim(),
            Note = NoteTextBox.Text.Trim(),
            IsDone = IsDoneCheckBox.IsChecked == true
        };
    }

    private void ClearTaskEditor()
    {
        _selectedTask = null;
        TasksDataGrid.SelectedItem = null;
        TitleTextBox.Text = string.Empty;
        AssigneeTextBox.Text = string.Empty;
        NoteTextBox.Text = string.Empty;
        IsDoneCheckBox.IsChecked = false;
    }
}
