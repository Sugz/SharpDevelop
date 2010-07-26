﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <author name="Kumar Devvrat"/>
//     <version>$Revision: $</version>
// </file>

using System;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.WpfDesign.Adorners;
using ICSharpCode.WpfDesign.Designer.Controls;
using ICSharpCode.WpfDesign.Extensions;

namespace ICSharpCode.WpfDesign.Designer.Extensions
{
    [ExtensionFor(typeof(FrameworkElement))]
    [ExtensionServer(typeof(PrimarySelectionExtensionServer))] 
    public class MarginHandleExtension : AdornerProvider
    {
    	 //TODO : Use array to store all the handles.
        private MarginHandle _leftHandle, _topHandle, _rightHandle, _bottomHandle;
        private Grid _grid;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (this.ExtendedItem.Parent != null)
            {
                if (this.ExtendedItem.Parent.ComponentType == typeof(Grid)){
                    FrameworkElement extendedControl = (FrameworkElement)this.ExtendedItem.Component;
                    AdornerPanel adornerPanel = new AdornerPanel();
                    
                    // If the Element is rotated/skewed in the grid, then margin handles do not appear
                    if (extendedControl.LayoutTransform.Value == Matrix.Identity && extendedControl.RenderTransform.Value == Matrix.Identity)
                    {
                    	  _grid = this.ExtendedItem.Parent.View as Grid;
                    	  
                        _leftHandle = new MarginHandle(this.ExtendedItem, adornerPanel, HandleOrientation.Left);
                        _topHandle = new MarginHandle(this.ExtendedItem, adornerPanel, HandleOrientation.Top);
                        _rightHandle = new MarginHandle(this.ExtendedItem, adornerPanel, HandleOrientation.Right);
                        _bottomHandle = new MarginHandle(this.ExtendedItem, adornerPanel, HandleOrientation.Bottom);
                        
                        _leftHandle.MouseLeftButtonDown += OnMouseDown;
                        _leftHandle.Stub.MouseLeftButtonDown += OnMouseDown;

                        _topHandle.MouseLeftButtonDown += OnMouseDown;
                        _topHandle.Stub.MouseLeftButtonDown += OnMouseDown;

                        _rightHandle.MouseLeftButtonDown += OnMouseDown;
                        _rightHandle.Stub.MouseLeftButtonDown += OnMouseDown;

                        _bottomHandle.MouseLeftButtonDown += OnMouseDown;
                        _bottomHandle.Stub.MouseLeftButtonDown += OnMouseDown;
                    }
                    
                    if (adornerPanel != null)
                        this.Adorners.Add(adornerPanel);
                }
            }
        }
        
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var row = (int) this.ExtendedItem.Properties.GetAttachedProperty(Grid.RowProperty).ValueOnInstance;
            var rowSpan = (int) this.ExtendedItem.Properties.GetAttachedProperty(Grid.RowSpanProperty).ValueOnInstance;

            var column = (int) this.ExtendedItem.Properties.GetAttachedProperty(Grid.ColumnProperty).ValueOnInstance;
            var columnSpan = (int) this.ExtendedItem.Properties.GetAttachedProperty(Grid.ColumnSpanProperty).ValueOnInstance;

            var margin = (Thickness) this.ExtendedItem.Properties[FrameworkElement.MarginProperty].ValueOnInstance;

            var point = this.ExtendedItem.View.TranslatePoint(new Point(), _grid);
            var position = new Rect(point, this.ExtendedItem.View.RenderSize);
            MarginHandle handle = null;
            if (sender is MarginHandle)
                handle = sender as MarginHandle;
            if (sender is MarginStub)
                handle = ((MarginStub) sender).Handle;
            if (handle != null) {
                switch (handle.Orientation) {
                    case HandleOrientation.Left:
                        if (_rightHandle.Visibility == Visibility.Visible) {
                            if (_leftHandle.Visibility == Visibility.Visible) {
                                margin.Left = 0;
                                this.ExtendedItem.Properties[FrameworkElement.WidthProperty].SetValue(position.Width);
                                this.ExtendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Right);                                
                            } else {
                                var leftMargin = position.Left - GetColumnOffset(column);
                                margin.Left = leftMargin;
                                this.ExtendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Stretch);                                
                            }
                        } else {
                            if (_leftHandle.Visibility == Visibility.Visible) {
                                margin.Left = 0;
                                var rightMargin = GetColumnOffset(column + columnSpan) - position.Right;
                                margin.Right = rightMargin;

                                this.ExtendedItem.Properties[FrameworkElement.WidthProperty].SetValue(position.Width);
                                this.ExtendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Right);                                
                            } else {
                                var leftMargin = position.Left - GetColumnOffset(column);
                                margin.Left = leftMargin;
                                this.ExtendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Left);                                
                            }
                        }
                        break;
                    case HandleOrientation.Top:
                        if (_bottomHandle.Visibility == Visibility.Visible) {
                            if (_topHandle.Visibility == Visibility.Visible) {
                                margin.Top = 0;
                                this.ExtendedItem.Properties[FrameworkElement.HeightProperty].SetValue(position.Height);
                                this.ExtendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].SetValue(VerticalAlignment.Bottom);
                            } else {
                                var topMargin = position.Top - GetRowOffset(row);
                                margin.Top = topMargin;
                                this.ExtendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].SetValue(VerticalAlignment.Stretch);
                            }
                        } else {
                            if (_topHandle.Visibility == Visibility.Visible) {
                                margin.Top = 0;
                                var bottomMargin = GetRowOffset(row + rowSpan) - position.Bottom;
                                margin.Bottom = bottomMargin;

                                this.ExtendedItem.Properties[FrameworkElement.HeightProperty].SetValue(position.Height);
                                this.ExtendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].SetValue(VerticalAlignment.Bottom);
                            } else {
                                var topMargin = position.Top - GetRowOffset(row);
                                margin.Top = topMargin;
                                this.ExtendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].SetValue(VerticalAlignment.Top);
                            }
                        }
                        break;
                    case HandleOrientation.Right:
                        if (_leftHandle.Visibility == Visibility.Visible) {
                            if (_rightHandle.Visibility == Visibility.Visible) {
                                margin.Right = 0;
                                this.ExtendedItem.Properties[FrameworkElement.WidthProperty].SetValue(position.Width);
                                this.ExtendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Left);                                
                            } else {
                                var rightMargin = GetColumnOffset(column + columnSpan) - position.Right;
                                margin.Right = rightMargin;
                                this.ExtendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Stretch);                                
                            }
                        } else {
                            if (_rightHandle.Visibility == Visibility.Visible) {
                                margin.Right = 0;
                                var leftMargin = position.Left - GetColumnOffset(column);
                                margin.Left = leftMargin;

                                this.ExtendedItem.Properties[FrameworkElement.WidthProperty].SetValue(position.Width);
                                this.ExtendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Left);                                
                            } else {
                                var rightMargin = GetColumnOffset(column + columnSpan) - position.Right;
                                margin.Right = rightMargin;
                                this.ExtendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Right);                                
                            }
                        }
                        break;
                    case HandleOrientation.Bottom:
                        if (_topHandle.Visibility == Visibility.Visible) {
                            if (_bottomHandle.Visibility == Visibility.Visible) {
                                margin.Bottom = 0;
                                this.ExtendedItem.Properties[FrameworkElement.HeightProperty].SetValue(position.Height);
                                this.ExtendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].SetValue(VerticalAlignment.Top);
                            } else {
                                var bottomMargin = GetRowOffset(row + rowSpan) - position.Bottom;
                                margin.Bottom = bottomMargin;
                                this.ExtendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].SetValue(VerticalAlignment.Stretch);
                            }
                        } else {
                            if (_bottomHandle.Visibility == Visibility.Visible) {
                                margin.Bottom = 0;
                                var topMargin = position.Top - GetRowOffset(row);
                                margin.Top = topMargin;

                                this.ExtendedItem.Properties[FrameworkElement.HeightProperty].SetValue(position.Height);
                                this.ExtendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].SetValue(VerticalAlignment.Top);
                            } else {
                                var bottomMargin = GetRowOffset(row + rowSpan) - position.Bottom;
                                margin.Bottom = bottomMargin;
                                this.ExtendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].SetValue(VerticalAlignment.Bottom);
                            }
                        }
                        break;
                }
            }
            this.ExtendedItem.Properties[FrameworkElement.MarginProperty].SetValue(margin);
        }
        
        private double GetColumnOffset(int index)
        {
            if (_grid != null) {
                // when the grid has no columns, we still need to return 0 for index=0 and grid.Width for index=1
                if (index == 0)
                    return 0;
                if (index < _grid.ColumnDefinitions.Count)
                    return _grid.ColumnDefinitions[index].Offset;
                return _grid.ActualWidth;
            }
            return 0;
        }

        private double GetRowOffset(int index)
        {
            if (_grid != null) {
                if (index == 0)
                    return 0;
                if (index < _grid.RowDefinitions.Count)
                    return _grid.RowDefinitions[index].Offset;
                return _grid.ActualHeight;
            }
            return 0;
        }
        
        public void HideHandles()
        {
        	if (_leftHandle != null && _topHandle != null && _rightHandle != null && _bottomHandle != null) {
        	_leftHandle.Visibility=Visibility.Hidden;
        	_leftHandle.ShouldBeVisible=false;
        	_topHandle.Visibility=Visibility.Hidden;
        	_topHandle.ShouldBeVisible=false;
        	_rightHandle.Visibility=Visibility.Hidden;
        	_rightHandle.ShouldBeVisible=false;
        	_bottomHandle.Visibility=Visibility.Hidden;
        	_bottomHandle.ShouldBeVisible=false;
        	}        	
        }
        
        public void ShowHandles()
        {
        	if (_leftHandle != null && _topHandle != null && _rightHandle != null && _bottomHandle != null){
        	_leftHandle.Visibility=Visibility.Visible;
        	_leftHandle.ShouldBeVisible=true;
        	_leftHandle.DecideVisiblity(_leftHandle.HandleLength);
        	_topHandle.Visibility=Visibility.Visible;
        	_topHandle.ShouldBeVisible=true;
        	_rightHandle.DecideVisiblity(_rightHandle.HandleLength);
        	_rightHandle.Visibility=Visibility.Visible;
        	_rightHandle.ShouldBeVisible=true;
        	_rightHandle.DecideVisiblity(_rightHandle.HandleLength);
        	_bottomHandle.Visibility=Visibility.Visible; 
        	_bottomHandle.ShouldBeVisible=true;
        	_bottomHandle.DecideVisiblity(_bottomHandle.HandleLength);
        	}
        }
    }
}
   
