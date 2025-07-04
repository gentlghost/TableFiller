﻿using Microsoft.Win32;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TableFiller;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly List<TextBox> textBoxes;
    private readonly ContextMenu? cmCharacterInput;

    private string? currentPath = null;
    private string? currentDirectory = null;

    public MainWindow()
    {
        InitializeComponent();
        textBoxes = GetTextBoxes();

        cmCharacterInput = FindResource("CmCharacterInput") as ContextMenu;

        // Sets the MaxLength and MaxLines to 1 because I am lazy to go through 256 textboxes
        foreach (var box in textBoxes)
        {
            box.MaxLength = 1;
            box.MaxLines = 1;
            box.ContextMenu = cmCharacterInput;
            box.MouseRightButtonDown += TxtCharacterInput_MouseRightButtonDown;
        }

        // Add commands
        CommandBinding cmdSave = new CommandBinding(ApplicationCommands.Save, SaveCommandHandler);
        CommandBinding cmdOpen = new CommandBinding(ApplicationCommands.Open, OpenCommandHandler);
        CommandBinding cmdNew = new CommandBinding(ApplicationCommands.New, NewCommandHandler);

        CommandBindings.Add(cmdSave);
        CommandBindings.Add(cmdOpen);
        CommandBindings.Add(cmdNew);
    }

    private void CreateTableBtn_Click(object sender, RoutedEventArgs e)
    {
        bool result = SaveTable();

        if (!result)
        {
            MessageBox.Show("Error: Saving to table failed", "Error 5", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (currentPath == null || currentDirectory == null)
            return;

        MessageBox.Show(".TBL file completed.");

        if (System.IO.File.Exists(currentPath))
        {
            Process.Start("explorer.exe", currentDirectory);
        }
        else
        {
            MessageBox.Show("Error: File path unknown.", "Error 3", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TxtCharacterInput_MouseRightButtonDown(object sender, RoutedEventArgs e)
    {
        if (cmCharacterInput != null)
        { 
            cmCharacterInput.PlacementTarget = sender as TextBox;
            cmCharacterInput.IsOpen = true;
        }
    }

    private List<TextBox> GetTextBoxes()
    {
        var list = new List<TextBox>();

        foreach (var child in grid1.Children)
        {
            if (child is TextBox)
            {
                list.Add((TextBox)child);
            }
        }

        return list;
    }

    /// Text context menu methods
    private void CopyItem_Click(object sender, RoutedEventArgs e)
    {
        if (cmCharacterInput?.PlacementTarget is TextBox)
        {
            ((TextBox)cmCharacterInput.PlacementTarget).Copy();
        }
    }

    private void PasteItem_Click(object sender, RoutedEventArgs e)
    {
        if (cmCharacterInput?.PlacementTarget is TextBox)
        {
            ((TextBox)cmCharacterInput.PlacementTarget).Paste();
        }
    }

    private void UppercaseItem_Click(object sender, RoutedEventArgs e)
    {
        AutomateCharacters('A', 'Z');
    }

    private void LowercaseItem_Click(object sender, RoutedEventArgs e)
    {
        AutomateCharacters('a', 'z');
    }

    private void NumberItem_Click(object sender, RoutedEventArgs e)
    {
        AutomateCharacters('0', '9');
    }

    private void HiraganaItem_Click(object sender, RoutedEventArgs e)
    {
        int start = AutomateCharacters('あ', 'お', -1, 2);
        start = AutomateCharacters('か', 'ち', start, 2);
        start = AutomateCharacters('つ', 'と', start, 2);
        start = AutomateCharacters('な', 'の', start);
        start = AutomateCharacters('は', 'ほ', start, 3);
        start = AutomateCharacters('ま', 'も', start);
        start = AutomateCharacters('や', 'よ', start, 2);
        start = AutomateCharacters('ら', 'ろ', start, 2);
        start = AutomateCharacters('わ', 'わ', start);
        AutomateCharacters('を', 'ん', start);
    }

    private void HiraganaDakutenItem_Click(object sender, RoutedEventArgs e)
    {
        int start = AutomateCharacters('が', 'ぢ', -1, 2);
        start = AutomateCharacters('づ', 'ど', start, 2);
        start = AutomateCharacters('ば', 'ぼ', start, 3);
        AutomateCharacters('ぱ', 'ぽ', start, 3);
    }

    private void HiraganaLittleItem_Click(object sender, RoutedEventArgs e)
    {
        int start = AutomateCharacters('ぁ', 'ぉ', -1, 2);
        start = AutomateCharacters('ゃ', 'ょ', start, 2);
        AutomateCharacters('っ', 'っ', start);
    }

    private void KatakanaItem_Click(object sender, RoutedEventArgs e)
    {
        int start = AutomateCharacters('ア', 'オ', -1, 2);
        start = AutomateCharacters('カ', 'チ', start, 2);
        start = AutomateCharacters('ツ', 'ト', start, 2);
        start = AutomateCharacters('ナ', 'ノ', start);
        start = AutomateCharacters('ハ', 'ホ', start, 3);
        start = AutomateCharacters('マ', 'モ', start);
        start = AutomateCharacters('ヤ', 'ヨ', start, 2);
        start = AutomateCharacters('ラ', 'ロ', start, 2);
        start = AutomateCharacters('ワ', 'ワ', start);
        AutomateCharacters('ヲ', 'ン', start);
    }

    private void KatakanaDakutenItem_Click(object sender, RoutedEventArgs e)
    {
        int start = AutomateCharacters('ガ', 'ヂ', offset: 2);
        start = AutomateCharacters('ヅ', 'ド', start, 2);
        start = AutomateCharacters('バ', 'ボ', start, 3);
        AutomateCharacters('パ', 'ポ', start, 3);
    }

    private void KatakanaLittleItem_Click(object sender, RoutedEventArgs e)
    {
        int start = AutomateCharacters('ァ', 'ォ', -1, 2);
        start = AutomateCharacters('ャ', 'ョ', start, 2);
        AutomateCharacters('ッ', 'ッ', start);
    }

    private int AutomateCharacters(char starting, char ending, int startingIndex = -1, int offset = 1)
    {
        TextBox? targetTextBox = cmCharacterInput?.PlacementTarget as TextBox;
        if (targetTextBox == null)
        { 
            return -1;    
        }

        int i, letter, si = (startingIndex != -1)? startingIndex : textBoxes.IndexOf(targetTextBox);
        for (i = si, letter = starting; letter <= ending; i++, letter += offset)
        {
            textBoxes[i].Text = "" + char.ConvertFromUtf32(letter);
        }

        return i;
    }

    /// Menubar methods
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        bool result = SaveTable();

        if (!result)
        {
            MessageBox.Show("Error: Saving to table failed", "Error 5", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        bool result = LoadTable();

        if (!result)
        {
            MessageBox.Show("Error: File failed to load", "Error 6", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        NewTable();
    }

    /// Command functions
    private void SaveCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        bool result = SaveTable();

        if (!result)
        {
            MessageBox.Show("Error: Saving to table failed", "Error 5", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        bool result = LoadTable();

        if (!result)
        {
            MessageBox.Show("Error: File failed to load", "Error 6", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void NewCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        NewTable();
    }

    /// Utility functions
    private void ClearTable()
    {
        foreach (var box in textBoxes)
        {
            box.Text = "";
        }
    }

    private void NewTable()
    {
        MessageBoxResult result = MessageBox.Show("This action will clear the entire table.\nAre you sure?", "Confirmation",
            MessageBoxButton.YesNo, MessageBoxImage.Information);

        if (result == MessageBoxResult.No)
            return;

        ClearTable();

        currentPath = null;
        currentDirectory = null;
    }

    private bool SaveTable()
    {
        if (currentPath == null || currentDirectory == null)
        { 
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".tbl";
            dialog.Filter = "(.tbl)|*.tbl";

            bool? result = dialog.ShowDialog();

            if (result is null)
            {
                MessageBox.Show("Error: Dialog failed to open.", "Error 2", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (result == false)
            {
                // This needs to return true because the dialogue was successful - user just cancelled action.
                return true;
            }

            currentPath = dialog.FileName;
            currentDirectory = System.IO.Path.GetDirectoryName(currentPath);
        }

        if (currentPath == null)
            return false;

        // Create a file
        using (StreamWriter file = new StreamWriter(currentPath))
        {
            // For every textbox in text boxes
            foreach (var box in textBoxes)
            {
                // If a textbox is empty, skip over it.
                if (string.IsNullOrEmpty(box.Text))
                {
                    continue;
                }

                // Get the position from the textbox name
                string position = box.Name.Remove(0, 1);
                file.WriteLine($"{position}={box.Text}");
            }
        }

        return true;
    }

    private bool LoadTable()
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.DefaultExt = ".tbl";
        dialog.Filter = "(.tbl)|*.tbl";

        bool? result = dialog.ShowDialog();

        if (result is null)
        {
            MessageBox.Show("Error: Dialog failed to open.", "Error 2", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (result == false)
        {
            return true;
        }

        ClearTable();

        currentPath = dialog.FileName;
        currentDirectory = System.IO.Path.GetDirectoryName(currentPath);
        try
        {
            // Read the file
            using (StreamReader reader = new StreamReader(currentPath))
            {
                string? line;
                // Get and read each line
                while ((line = reader.ReadLine()) != null)
                {
                    string[] subString = line.Split('=', 2);
                    // Get the first text box that has the position name
                    var textBox = textBoxes.FirstOrDefault(tb => tb.Name.Contains(subString[0]));
                    if (textBox != null)
                    {
                        textBox.Text = subString[1];
                    }
                }
            }
        }
        catch (Exception)
        {
            MessageBox.Show("Error: File failed to load.", "Error 4", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        return true;
    }
}