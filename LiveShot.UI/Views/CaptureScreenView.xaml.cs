﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using LiveShot.API;
using LiveShot.API.Drawing;
using LiveShot.API.Events.Input;
using LiveShot.API.Utils;
using LiveShot.UI.Controls.Button;
using Microsoft.Extensions.DependencyInjection;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace LiveShot.UI.Views
{
    public partial class CaptureScreenView
    {
        private readonly IEventPipeline _events;
        private readonly IServiceProvider _services;

        private ExportWindowView? _exportWindow;
        private ActionButton? _activeTool;
        private Bitmap? _screenShot;

        public CaptureScreenView(IEventPipeline events, IServiceProvider services, IEnumerable<IDrawingTool> tools)
        {
            InitializeComponent();

            _events = events;
            _services = services;

            Top = SystemParameters.VirtualScreenTop;
            Left = SystemParameters.VirtualScreenLeft;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            SelectCanvas.Width = Width;
            SelectCanvas.Height = Height;
            SelectCanvas.WithEvents(events);

            DrawingCanvas.With(tools, events);

            CanvasRightPanel.With(events, Width, Height);
            CanvasBottomPanel.With(events, Width, Height);

            var actionButtons = WindowUtils.FindVisualChildren<ActionButton>(SelectCanvas).ToList();

            foreach (var actionButton in actionButtons)
                actionButton.Click += (actual, _) => ActionButtonOnClick(actual, actionButtons);

            ColorPickerBtn.Click += ColorPickerBtnOnClick;
            UndoBtn.Click += UndoBtnOnClick;

            UploadBtn.Click += UploadBtnOnClick;
            GoogleBtn.Click += GoogleBtnOnClick;
            CopyBtn.Click += CopyBtnOnClick;
            SaveBtn.Click += SaveBtnOnClick;
            CloseBtn.Click += CloseBtnOnClick;

            CaptureScreen();
        }

        private void UndoBtnOnClick(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void ColorPickerBtnOnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorDialog();

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            DrawingCanvas.DrawingColor = ColorUtils.GetBrushFromChannels(
                dialog.Color.R, dialog.Color.G, dialog.Color.B, dialog.Color.A
            );

            _activeTool?.UpdateIconFill(DrawingCanvas.DrawingColor);
        }

        private void ActionButtonOnClick(object sender, IEnumerable<ActionButton> all)
        {
            foreach (var button in all)
            {
                if (button == sender)
                {
                    button.IsActive = !button.IsActive;

                    DrawingCanvas.Tool = button.IsActive ? button.ActiveTool : CanvasTool.Default;

                    _activeTool = button;
                    _activeTool.UpdateIconFill(DrawingCanvas.DrawingColor);

                    continue;
                }

                button.IsActive = false;
                button.UpdateIconFill(DrawingCanvas.DrawingColor);
            }
        }

        private void GoogleBtnOnClick(object sender, RoutedEventArgs e)
        {
            OpenExportWindow(true);
        }

        private void CloseBtnOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveBtnOnClick(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }

        private void CopyBtnOnClick(object sender, RoutedEventArgs e)
        {
            CopyImage();
        }

        private void UploadBtnOnClick(object sender, RoutedEventArgs e)
        {
            OpenExportWindow();
        }

        private void Undo()
        {
            DrawingCanvas.Undo();
        }

        private void CaptureScreen()
        {
            (int screenTop, int screenLeft, int screenWidth, int screenHeight) =
                ((int, int, int, int)) (Top, Left, Width, Height);

            var bitmap = ImageUtils.CaptureScreen(screenWidth, screenHeight, screenLeft, screenTop);
            var bitmapSource = ImageUtils.GetBitmapSource(bitmap);

            _screenShot = bitmap;

            SelectCanvas.Background = new ImageBrush(bitmapSource);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.C:
                    if (KeyBoardUtils.IsCtrlPressed)
                        CopyImage();
                    break;
                case Key.S:
                    if (KeyBoardUtils.IsCtrlPressed)
                        SaveImage();
                    break;
                case Key.D:
                    if (KeyBoardUtils.IsCtrlPressed)
                        OpenExportWindow();
                    break;
                case Key.Z:
                    if (KeyBoardUtils.IsCtrlPressed)
                        Undo();
                    break;
            }

            _events.Dispatch<OnKeyDown>(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            _events.Dispatch<OnKeyUp>(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _events.Dispatch<OnMouseWheel>(e);
        }

        private void SaveImage()
        {
            var selection = SelectCanvas.Selection;

            if (_screenShot is null || selection is null || selection.Invalid) return;

            bool saved = FileUtils.SaveImage(selection, _screenShot, ImageUtils.GetBitmapFromCanvas(SelectCanvas));

            if (saved)
                Close();
        }

        private void OpenExportWindow(bool google = false)
        {
            if (_exportWindow is not null || _screenShot is null) return;

            var selection = SelectCanvas.Selection;

            if (selection is null || selection.Invalid) return;

            var bitmap = ImageUtils.GetBitmap(selection, _screenShot, ImageUtils.GetBitmapFromCanvas(SelectCanvas));

            _exportWindow = _services.GetService<ExportWindowView>();

            if (_exportWindow is null) return;

            _exportWindow.Show();

            double x = Width - _exportWindow.Width - 100;
            double y = Height - _exportWindow.Height - 100;

            _exportWindow.Left = x;
            _exportWindow.Top = y;

            _exportWindow.Upload(bitmap, google);

            Close();
        }

        private void CopyImage()
        {
            var selection = SelectCanvas.Selection;
            if (selection is null || selection.Invalid || _screenShot is null) return;

            bool copied = ImageUtils.CopyImage(selection, _screenShot, ImageUtils.GetBitmapFromCanvas(SelectCanvas));

            if (copied) Close();
        }
    }
}