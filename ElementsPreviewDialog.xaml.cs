using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Dialog for previewing and selecting UI elements
    /// </summary>
    public partial class ElementsPreviewDialog : Window
    {
        // Result properties
        public List<UIElementInfo> SelectedElements { get; private set; } = new List<UIElementInfo>();
        
        private ListView _listView;

        public ElementsPreviewDialog(List<UIElementInfo> elements, string windowTitle)
        {
            Title = $"UI Elements - {windowTitle}";
            Width = 700;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            
            // Create UI
            var grid = new Grid();
            grid.Margin = new Thickness(10);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Header
            var header = new TextBlock
            {
                Text = $"Interactive UI elements in \"{windowTitle}\":" +
                      "\n(Double-click on an element to add it to your automation, or select multiple elements and click 'Add Selected')",
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(header, 0);
            grid.Children.Add(header);
            
            // ListView
            _listView = new ListView();
            _listView.SelectionMode = SelectionMode.Extended; // Allow multiple selection
            _listView.MouseDoubleClick += ListView_MouseDoubleClick;
            
            var gridView = new GridView();
            
            gridView.Columns.Add(new GridViewColumn 
            { 
                Header = "Name", 
                DisplayMemberBinding = new Binding("Name"),
                Width = 200
            });
            
            gridView.Columns.Add(new GridViewColumn 
            { 
                Header = "Automation ID", 
                DisplayMemberBinding = new Binding("AutomationId"),
                Width = 150
            });
            
            gridView.Columns.Add(new GridViewColumn 
            { 
                Header = "Type", 
                DisplayMemberBinding = new Binding("ControlTypeName"),
                Width = 150
            });
            
            gridView.Columns.Add(new GridViewColumn 
            { 
                Header = "Class Name", 
                DisplayMemberBinding = new Binding("ClassName"),
                Width = 150
            });
            
            _listView.View = gridView;
            _listView.ItemsSource = elements;
            
            Grid.SetRow(_listView, 1);
            grid.Children.Add(_listView);
            
            // Buttons panel
            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            var addSelectedButton = new Button
            {
                Content = "Add Selected",
                Width = 120,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0)
            };
            addSelectedButton.Click += AddSelectedButton_Click;
            buttonsPanel.Children.Add(addSelectedButton);
            
            var closeButton = new Button
            {
                Content = "Close",
                Width = 80,
                Height = 30,
                IsCancel = true
            };
            closeButton.Click += (sender, e) => Close();
            buttonsPanel.Children.Add(closeButton);
            
            Grid.SetRow(buttonsPanel, 2);
            grid.Children.Add(buttonsPanel);
            
            Content = grid;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_listView.SelectedItem is UIElementInfo selectedElement)
            {
                // Add the selected element to the results
                SelectedElements.Add(selectedElement);

                // Show confirmation
                TimedMessageBox.Show($"Element '{selectedElement}' added to your automation.", 
                              "Element Added", 5000);
            }
        }

        private void AddSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            // Get all selected items
            var selectedItems = _listView.SelectedItems;
            
            if (selectedItems.Count == 0)
            {
                TimedMessageBox.Show("Please select at least one element to add.", 
                              "No Selection", 5000);
                return;
            }
            
            // Add all selected elements to the results
            foreach (UIElementInfo item in selectedItems)
            {
                SelectedElements.Add(item);
            }

            // Show confirmation
            TimedMessageBox.Show(
                $"{selectedItems.Count} element(s) added to your automation.", 
                          "Elements Added", 
                          5000);
        }
    }
}
