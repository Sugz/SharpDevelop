﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the BSD license (for details please see \src\AddIns\Debugger\Debugger.AddIn\license.txt)

using Debugger.AddIn.Visualizers.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Debugger.AddIn.Visualizers.Common;
using Debugger.AddIn.Visualizers.Graph.Layout;

namespace Debugger.AddIn.Visualizers.Graph.Drawing
{
	/// <summary>
	/// Interaction logic for PositionedGraphNodeControl.xaml
	/// </summary>
	public partial class PositionedGraphNodeControl : UserControl
	{
		/// <summary>
		/// Occurs when <see cref="PositionedNodeProperty"/> is expanded.
		/// </summary>
		public event EventHandler<PositionedPropertyEventArgs> PropertyExpanded;
		/// <summary>
		/// Occurs when <see cref="PositionedNodeProperty"/> is collaped.
		/// </summary>
		public event EventHandler<PositionedPropertyEventArgs> PropertyCollapsed;
		
		/// <summary>
		/// Occurs when <see cref="NesteNodeViewModel"/> is expanded.
		/// </summary>
		public event EventHandler<ContentNodeEventArgs> ContentNodeExpanded;
		/// <summary>
		/// Occurs when <see cref="NesteNodeViewModel"/> is collaped.
		/// </summary>
		public event EventHandler<ContentNodeEventArgs> ContentNodeCollapsed;
		
		
		// shown in the ListView
		private ObservableCollection<ContentNode> items = new ObservableCollection<ContentNode>();
		
		/// <summary>
		/// The tree to be displayed in this Control.
		/// </summary>
		public ContentNode Root { get; set; }
		
		/// <summary>
		/// Sets the node to be displayed by this control.
		/// </summary>
		/// <param name="node"></param>
		public void SetDataContext(PositionedNode node)
		{
			this.DataContext = node;
			this.Root = node.Content;
			this.items = GetInitialItems(this.Root);
			// data virtualization, ContentPropertyNode implements IEvaluate
			this.listView.ItemsSource = new VirtualizingObservableCollection<ContentNode>(this.items);
		}
		
		public void CalculateWidthHeight()
		{
			int nameColumnMaxLen = this.items.MaxOrDefault(contentNode => contentNode.Name.Length, 0);
			GridView gv = listView.View as GridView;
			gv.Columns[1].Width = Math.Min(20 + nameColumnMaxLen * 6, 260);
			gv.Columns[2].Width = 80;
			listView.Width = gv.Columns[0].Width + gv.Columns[1].Width + gv.Columns[2].Width + 10;
			
			int maxItems = 10;
			listView.Height = 4 + Math.Min(this.items.Count, maxItems) * 20;
			if (this.items.Count > maxItems) {
				listView.Width += 30;	// for scrollbar
			}
			
			this.Width = listView.Width + 2;
			this.Height = listView.Height + this.typeNameHeaderBorder.Height + 2;
		}
		
		public PositionedGraphNodeControl()
		{
			InitializeComponent();
			PropertyExpanded = null;
			PropertyCollapsed = null;
			ContentNodeExpanded = null;
			ContentNodeCollapsed = null;
			this.listView.ItemsSource = null;
		}
		
		void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			/*var clickedText = (TextBlock)e.Source;
			var clickedNode = (ContentNode)(clickedText).DataContext;
			var propNode = clickedNode as ContentPropertyNode;
			if (propNode != null && propNode.Property != null && propNode.Property.Edge != null && propNode.Property.Edge.Spline != null)
			{
				propNode.Property.Edge.Spline.StrokeThickness = propNode.Property.Edge.Spline.StrokeThickness + 1;
			}*/
		}
		
		private void PropertyExpandButton_Click(object sender, RoutedEventArgs e)
		{
			var clickedButton = (ToggleButton)e.Source;
			ContentPropertyNode clickedNode = null;
			try
			{
				clickedNode = (ContentPropertyNode)(clickedButton).DataContext;
			}
			catch(InvalidCastException)
			{
				throw new InvalidOperationException("Clicked property expand button, button shouln't be there - DataContext is not PropertyNodeViewModel.");
			}
			
			PositionedNodeProperty property = clickedNode.Property;
			clickedButton.Content = property.IsPropertyExpanded ? "-" : "+";
			
			if (property.IsPropertyExpanded)
			{
				OnPropertyExpanded(property);
			}
			else
			{
				OnPropertyCollapsed(property);
			}
		}
		
		private void NestedExpandButton_Click(object sender, RoutedEventArgs e)
		{
			var clickedButton = (ToggleButton)e.Source;
			var clickedNode = (ContentNode)(clickedButton).DataContext;
			int clickedIndex = this.items.IndexOf(clickedNode);
			clickedButton.Content = clickedNode.IsExpanded ? "-" : "+";	// could be done by a converter

			if (clickedNode.IsExpanded)
			{
				// insert children
				int i = 1;
				foreach (var childNode in clickedNode.Children)
				{
					this.items.Insert(clickedIndex + i, childNode);
					i++;
				}
				OnContentNodeExpanded(clickedNode);
			}
			else
			{
				// remove whole subtree
				int size = SubtreeSize(clickedNode) - 1;
				for (int i = 0; i < size; i++)
				{
					this.items.RemoveAt(clickedIndex + 1);
				}
				OnContentNodeCollapsed(clickedNode);
			}
			
			CalculateWidthHeight();
		}
		
		ObservableCollection<ContentNode> GetInitialItems(ContentNode root)
		{
			return new ObservableCollection<ContentNode>(root.FlattenChildrenExpanded());
		}
		
		int SubtreeSize(ContentNode node)
		{
			return 1 + node.Children.Sum(child => (child.IsExpanded ? SubtreeSize(child) : 1));
		}
		
		#region event helpers
		protected virtual void OnPropertyExpanded(PositionedNodeProperty property)
		{
			if (this.PropertyExpanded != null)
				this.PropertyExpanded(this, new PositionedPropertyEventArgs(property));
		}

		protected virtual void OnPropertyCollapsed(PositionedNodeProperty property)
		{
			if (this.PropertyCollapsed != null)
				this.PropertyCollapsed(this, new PositionedPropertyEventArgs(property));
		}
		
		protected virtual void OnContentNodeExpanded(ContentNode node)
		{
			if (this.ContentNodeExpanded != null)
				this.ContentNodeExpanded(this, new ContentNodeEventArgs(node));
		}

		protected virtual void OnContentNodeCollapsed(ContentNode node)
		{
			if (this.ContentNodeCollapsed != null)
				this.ContentNodeCollapsed(this, new ContentNodeEventArgs(node));
		}
		
		void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
		{
			SetEdgeStrokeThickness((ListViewItem)e.Source, 2);
		}

		void ListViewItem_MouseLeave(object sender, MouseEventArgs e)
		{
			SetEdgeStrokeThickness((ListViewItem)e.Source, 1);
		}
		
		void SetEdgeStrokeThickness(ListViewItem listViewItem, int thickness)
		{
			var clickedNode = (ContentNode)listViewItem.DataContext;
			var propNode = clickedNode as ContentPropertyNode;
			if (propNode != null && propNode.Property != null && propNode.Property.Edge != null && propNode.Property.Edge.Spline != null) {
				propNode.Property.Edge.Spline.StrokeThickness = thickness;
			}
		}
		#endregion
	}
}
