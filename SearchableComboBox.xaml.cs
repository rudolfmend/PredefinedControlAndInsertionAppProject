using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// ComboBox s možnosťou vyhľadávania
    /// </summary>
    public partial class SearchableComboBox : UserControl
    {
        private IEnumerable<AppUIElement> _allItems = Array.Empty<AppUIElement>();
        private ObservableCollection<AppUIElement> _filteredItems;

        // Event pre oznámenie zmeny výberu
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        // Vlastnosť pre vybraný prvok
        public AppUIElement? SelectedItem
        {
            get { return cmbItems.SelectedItem as AppUIElement; }
            set { cmbItems.SelectedItem = value; }
        }

        // Vlastnosť pre všetky položky
        public IEnumerable<AppUIElement> ItemsSource
        {
            get { return _allItems; }
            set
            {
                _allItems = value;
                _filteredItems = new ObservableCollection<AppUIElement>(_allItems);
                cmbItems.ItemsSource = _filteredItems;
            }
        }

        public SearchableComboBox()
        {
            InitializeComponent();
            _filteredItems = new ObservableCollection<AppUIElement>();
        }

        // Metóda pre filtrovanie položiek
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allItems == null) return;

            string searchText = txtSearch.Text.Trim().ToLower();

            _filteredItems.Clear();

            // Ak je vyhľadávací text prázdny, zobraziť všetky položky
            if (string.IsNullOrEmpty(searchText))
            {
                foreach (var item in _allItems)
                {
                    _filteredItems.Add(item);
                }
            }
            else
            {
                // Inak filtrovať podľa vyhľadávacieho textu
                foreach (var item in _allItems)
                {
                    if (item.Name.ToLower().Contains(searchText) ||
                        item.ElementType.ToLower().Contains(searchText) ||
                        item.AutomationId.ToLower().Contains(searchText))
                    {
                        _filteredItems.Add(item);
                    }
                }
            }
        }

        // Propagovať udalosť výberu
        private void CmbItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        // Metóda pre výber položky podľa mena
        public void SelectItemByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            foreach (var item in _filteredItems)
            {
                if (item.Name == name)
                {
                    cmbItems.SelectedItem = item;
                    break;
                }
            }
        }
    }
}
