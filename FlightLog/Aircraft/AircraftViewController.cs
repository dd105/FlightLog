// 
// AircraftViewController.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Jeffrey Stedfast
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 

using System;
using System.Drawing;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.SQLite;
using MonoTouch.UIKit;

namespace FlightLog {
	public class AircraftViewController : SQLiteTableViewController<Aircraft>, IComparer<Aircraft>
	{
		static NSString key = new NSString ("Aircraft");
		
		EditAircraftDetailsViewController editor;
		AircraftDetailsViewController details;
		UIBarButtonItem addAircraft;
		NSIndexPath updating;
		bool searching;
		
		public AircraftViewController (AircraftDetailsViewController details)
		{
			RowHeight = AircraftTableViewCell.CellHeight;
			SearchPlaceholder = "Search Aircraft";
			AutoHideSearch = true;
			Title = "Aircraft";
			
			this.details = details;
			
			LogBook.AircraftAdded += OnAircraftAdded;
			LogBook.AircraftUpdated += OnAircraftUpdated;
			LogBook.AircraftWillUpdate += OnAircraftWillUpdate;
			LogBook.AircraftUpdateFailed += OnAircraftUpdateFailed;
		}
		
		UITableView CurrentTableView {
			get {
				return searching ? SearchDisplayController.SearchResultsTableView : TableView;
			}
		}
		
		#region IComparer[Aircraft] implementation
		int IComparer<Aircraft>.Compare (Aircraft x, Aircraft y)
		{
			return x.Id - y.Id;
		}
		#endregion

		void OnAircraftAdded (object sender, AircraftEventArgs e)
		{
			var tableView = CurrentTableView;
			var model = ModelForTableView (tableView);
			
			details.Aircraft = e.Aircraft;
			model.ReloadData ();
			
			int index = model.IndexOf (e.Aircraft, this);
			
			if (index == -1) {
				// This suggests that we are probably displaying the search view and the
				// newly added aircraft does not match the search criteria.
				
				// Just reload the original TableView...
				Model.ReloadData ();
				TableView.ReloadData ();
				return;
			}
			
			int section, row;
			if (!model.IndexToSectionAndRow (index, out section, out row)) {
				// This shouldn't happen...
				model.ReloadData ();
				tableView.ReloadData ();
				return;
			}
			
			NSIndexPath path = NSIndexPath.FromRowSection (section, row);
			NSIndexPath[] rows = new NSIndexPath[1];
			rows[0] = path;
			
			// Add the row to the table...
			tableView.InsertRows (rows, UITableViewRowAnimation.Automatic);
			
			// Select and scroll to the newly added aircraft...
			
			// From Apple's documentation:
			//
			// To scroll to the newly selected row with minimum scrolling, select the row using
			// selectRowAtIndexPath:animated:scrollPosition: with UITableViewScrollPositionNone,
			// then call scrollToRowAtIndexPath:atScrollPosition:animated: with
			// UITableViewScrollPositionNone.
			tableView.SelectRow (path, true, UITableViewScrollPosition.None);
			tableView.ScrollToRow (path, UITableViewScrollPosition.None, true);
			path.Dispose ();
		}
		
		void OnAircraftWillUpdate (object sender, AircraftEventArgs e)
		{
			var tableView = CurrentTableView;
			var model = ModelForTableView (tableView);
			
			updating = PathForVisibleItem (tableView, e.Aircraft);
			if (updating != null) {
				// We're done.
				return;
			}
			
			// Otherwise we gotta do things the hard way...
			int index = model.IndexOf (e.Aircraft, this);
			int section, row;
			
			if (index == -1 || !model.IndexToSectionAndRow (index, out section, out row))
				return;
			
			updating = NSIndexPath.FromRowSection (row, section);
		}
		
		void OnAircraftUpdateFailed (object sender, AircraftEventArgs e)
		{
			if (updating != null) {
				updating.Dispose ();
				updating = null;
			}
		}
		
		void OnAircraftUpdated (object sender, AircraftEventArgs e)
		{
			if (updating == null) {
				// The user probably just saved an aircraft which doesn't match the search criteria.
				Model.ReloadData ();
				TableView.ReloadData ();
				return;
			}
			
			// No matter what, reload the main model.
			Model.ReloadData ();
			
			// Now update the UITableView that is currently being displayed.
			var tableView = CurrentTableView;
			var model = ModelForTableView (tableView);
			
			// The date may have changed, which means the position may have changed.
			model.ReloadData ();
			
			// Find the new position of the flight log entry.
			int index = model.IndexOf (e.Aircraft, this);
			int section, row;
			
			if (index == -1 || !model.IndexToSectionAndRow (index, out section, out row)) {
				// The aircraft no longer exists in this model (doesn't match search criteria?)
				NSIndexPath[] rows = new NSIndexPath[1];
				rows[0] = updating;
				
				tableView.DeleteRows (rows, UITableViewRowAnimation.Automatic);
			} else {
				// Reload the row.
				NSIndexPath[] rows = new NSIndexPath[1];
				rows[0] = updating;
				
				tableView.ReloadRows (rows, UITableViewRowAnimation.None);
			}
			
			// If the currently displayed UITableView isn't the main view, reset state of the main tableview.
			if (tableView != TableView)
				TableView.ReloadData ();
			
			updating.Dispose ();
			updating = null;
		}
		
		void OnAircraftDeleted (UITableView tableView, NSIndexPath indexPath)
		{
			var rows = new NSIndexPath[1];
			rows[0] = indexPath;
			
			// Reset the models...
			SearchModel.ReloadData ();
			Model.ReloadData ();
			
			tableView.DeleteRows (rows, UITableViewRowAnimation.Automatic);
			
			if (tableView != TableView) {
				// We've already deleted the item from the search model, but we
				// have no way of knowing its section/row in the normal model.
				TableView.ReloadData ();
			} else if (Model.SectionCount == 0) {
				OnAddClicked (null, null);
			}
		}
		
		void OnEditorClosed (object sender, EventArgs args)
		{
			editor = null;
		}
		
		void OnAddClicked (object sender, EventArgs args)
		{
			if (editor != null || details.EditorEngaged)
				return;
			
			editor = new EditAircraftDetailsViewController (new Aircraft (), false);
			editor.EditorClosed += OnEditorClosed;
			
			details.NavigationController.PushViewController (editor, true);
		}
		
		protected override SQLiteTableModel<Aircraft> CreateModel (bool forSearching)
		{
			var model = new SQLiteTableModel<Aircraft> (LogBook.SQLiteDB, 16);
			
			return model;
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			addAircraft = new UIBarButtonItem (UIBarButtonSystemItem.Add, OnAddClicked);
			NavigationItem.LeftBarButtonItem = addAircraft;
		}
		
		void SelectFirstOrAdd ()
		{
			if (Model.Count == 0) {
				// Add new aircraft...
				OnAddClicked (null, null);
			} else {
				// Select first flight...
				var visible = TableView.IndexPathsForVisibleRows;
				if (visible == null || visible.Length == 0)
					return;
				
				TableView.SelectRow (visible[0], false, UITableViewScrollPosition.None);
				RowSelected (TableView, visible[0]);
			}
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			
			if (searching)
				return;
			
			var path = TableView.IndexPathForSelectedRow;
			if (path != null)
				return;
			
			SelectFirstOrAdd ();
		}
		
		protected override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath, Aircraft aircraft)
		{
			AircraftTableViewCell cell = tableView.DequeueReusableCell (key) as AircraftTableViewCell;
			
			if (cell == null)
				cell = new AircraftTableViewCell (key);
			
			cell.Aircraft = aircraft;
			
			return cell;
		}
		
		protected override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{
			var model = ModelForTableView (tableView);
			var aircraft = model.GetItem (indexPath.Section, indexPath.Row);
			
			return LogBook.CanDelete (aircraft);
		}
		
		protected override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return UITableViewCellEditingStyle.Delete;
		}
		
		protected override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			if (editingStyle != UITableViewCellEditingStyle.Delete)
				return;
			
			Aircraft aircraft = GetItem (tableView, indexPath);
			
			if (!LogBook.Delete (aircraft))
				return;
			
			OnAircraftDeleted (tableView, indexPath);
		}
		
		protected override void DidBeginSearch (UISearchDisplayController controller)
		{
			searching = true;
		}
		
		protected override void DidEndSearch (UISearchDisplayController controller)
		{
			searching = false;
		}
		
		static bool PathsEqual (NSIndexPath path0, NSIndexPath path1)
		{
			return path0.Section == path1.Section && path0.Row == path1.Row;
		}
		
		protected override NSIndexPath WillSelectRow (UITableView tableView, NSIndexPath indexPath)
		{
			var selected = tableView.IndexPathForSelectedRow;
			if (selected != null && !PathsEqual (selected, indexPath))
				tableView.DeselectRow (selected, false);
			
			return indexPath;
		}
		
		protected override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			details.Aircraft = GetItem (tableView, indexPath);
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			if (updating != null) {
				updating.Dispose ();
				updating = null;
			}
			
			LogBook.AircraftAdded -= OnAircraftAdded;
			LogBook.AircraftUpdated -= OnAircraftUpdated;
			LogBook.AircraftWillUpdate -= OnAircraftWillUpdate;
			LogBook.AircraftUpdateFailed -= OnAircraftUpdateFailed;
		}
	}
}
