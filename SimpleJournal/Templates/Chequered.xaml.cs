﻿using SimpleJournal.Controls;
using SimpleJournal.Data;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleJournal.Templates
{
    /// <summary>
    /// Interaktionslogik für Chequered.xaml
    /// </summary>
    public partial class Chequered : Page, IPaper
    {
        public Chequered()
        {
            InitializeComponent();

            // Load the correct drawing brush for the canvas
            if (Settings.Instance.UseOldChequeredPattern)
            {
                if (FindResource("OldChequeredBrush") is DrawingBrush drawingBrush)
                    canvas.Background = drawingBrush;
            }
            else
            {
                if (FindResource("CurrentChequeredBrush") is DrawingBrush drawingBrush)
                    canvas.Background = drawingBrush;
            }
        }
        
        public Format Format => Format.A4;

        public PaperType Type => PaperType.Chequeued;

        public DrawingCanvas Canvas => canvas;

        public PageSplitter Border { get; set; }

        public IPaper ClonePage(bool isReadonly)
        {
            Chequered chq = new Chequered();

            if (isReadonly)
                chq.Canvas.EditingMode = InkCanvasEditingMode.None;
            chq.Canvas.Strokes = Canvas.Strokes.Clone();
            foreach (var child in Canvas.Children)
                chq.Canvas.Children.Add(GeneralHelper.CloneElement(child));

            return chq;
        }

        public void SetDebug(bool state = true)
        {
            Canvas.SetDebug(state);
        }
    }
}
