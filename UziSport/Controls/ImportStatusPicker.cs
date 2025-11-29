using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace UziSport.Controls
{
    public class ImportStatusItem
    {
        public ImportStatus Status { get; set; }
        public string Name { get; set; } = string.Empty;
        public Color Color { get; set; } = Colors.Black;
    }

    public class ImportStatusPicker : Picker
    {
        private readonly List<ImportStatusItem> _items = new()
        {
            new ImportStatusItem
            {
                Status = ImportStatus.InProgress,
                Name = "Đang xử lý",
                Color = Colors.DarkOrange
            },
            new ImportStatusItem
            {
                Status = ImportStatus.Completed,
                Name = "Hoàn thành",
                Color = Colors.Green
            },
            new ImportStatusItem
            {
                Status = ImportStatus.Cancelled,
                Name = "Hủy",
                Color = Colors.Red
            }
        };

        public static readonly BindableProperty SelectedStatusProperty =
            BindableProperty.Create(
                nameof(SelectedStatus),
                typeof(ImportStatus?),
                typeof(ImportStatusPicker),
                null,
                BindingMode.TwoWay,
                propertyChanged: OnSelectedStatusChanged);

        public ImportStatus? SelectedStatus
        {
            get => (ImportStatus?)GetValue(SelectedStatusProperty);
            set => SetValue(SelectedStatusProperty, value);
        }

        public ImportStatusPicker()
        {
            ItemsSource = _items;
            ItemDisplayBinding = new Binding(nameof(ImportStatusItem.Name));

            SelectedIndexChanged += OnSelectedIndexChanged;

            SelectedStatus = ImportStatus.InProgress;
            VerticalTextAlignment = TextAlignment.Center;
            HorizontalTextAlignment = TextAlignment.Center;

            SyncSelectedIndexWithStatus();

            UpdateVisual();
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndex < 0 || SelectedIndex >= _items.Count)
            {
                SelectedStatus = null;
                UpdateVisual();
                return;
            }

            var item = _items[SelectedIndex];
            SelectedStatus = item.Status;
            UpdateVisual(item);
        }

        private static void OnSelectedStatusChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ImportStatusPicker)bindable;
            picker.SyncSelectedIndexWithStatus();
        }

        private void SyncSelectedIndexWithStatus()
        {
            if (SelectedStatus is null)
            {
                SelectedIndex = -1;
                UpdateVisual();
                return;
            }

            var index = _items.FindIndex(x => x.Status == SelectedStatus.Value);
            if (index >= 0)
            {
                SelectedIndex = index;
                UpdateVisual(_items[index]);
            }
            else
            {
                SelectedIndex = -1;
                UpdateVisual();
            }
        }

        private void UpdateVisual(ImportStatusItem? item = null)
        {
            if (item == null && SelectedIndex >= 0 && SelectedIndex < _items.Count)
            {
                item = _items[SelectedIndex];
            }

            if (item == null)
            {
                BackgroundColor = Colors.Transparent;
                TextColor = Colors.Black;
                return;
            }

            var baseColor = item.Color;
            BackgroundColor = new Color(baseColor.Red, baseColor.Green, baseColor.Blue, 0.15f);
            TextColor = baseColor;
        }
    }
}
