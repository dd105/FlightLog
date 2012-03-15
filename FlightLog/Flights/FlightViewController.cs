// 
// FlightViewController.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Jeffrey Stedfast
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

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using MonoTouch.SQLite;

namespace FlightLog {
	public class FlightViewController : SQLiteTableViewController<Flight>
	{
		static SQLiteOrderBy orderBy = new SQLiteOrderBy ("Date", SQLiteOrderByDirection.Descending);
		static string sectionExpr = "strftime ('%Y', Date)";
		static NSString key = new NSString ("Flight");
		
		EditFlightDetailsViewController editor;
		NSIndexPath selected, searchSelected;
		FlightDetailsViewController details;
		UIBarButtonItem addFlight;
		
		public FlightViewController (FlightDetailsViewController details) :
			base (LogBook.SQLiteDB, 16, orderBy, sectionExpr)
		{
			SearchPlaceholder = "Search Flights";
			AutoHideSearch = true;
			Title = "Flights";
			
			this.details = details;
			
			LogBook.FlightAdded += OnFlightAdded;
			LogBook.FlightUpdated += OnFlightUpdated;
			LogBook.FlightDeleted += OnFlightDeleted;
		}
		
		void OnFlightAdded (object sender, FlightEventArgs e)
		{
			// FIXME: we probably want to select and scroll to this item
			ReloadData ();
		}
		
		void OnFlightUpdated (object sender, FlightEventArgs e)
		{
			ReloadRowForItem (SearchDisplayController.SearchResultsTableView, e.Flight);
			ReloadRowForItem (TableView, e.Flight);
		}
		
		void OnFlightDeleted (object sender, FlightEventArgs e)
		{
			// FIXME: instead of reloading the table data, we should probably
			// find a more optimal solution...
			ReloadData ();
		}
		
		void OnEditorClosed (object sender, EventArgs args)
		{
			editor = null;
		}
		
		void OnAddClicked (object sender, EventArgs args)
		{
			if (editor != null || details.EditorEngaged)
				return;
			
			editor = new EditFlightDetailsViewController (new Flight (DateTime.Today), false);
			editor.EditorClosed += OnEditorClosed;
			
			details.NavigationController.PushViewController (editor, true);
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			addFlight = new UIBarButtonItem (UIBarButtonSystemItem.Add, OnAddClicked);
			NavigationItem.LeftBarButtonItem = addFlight;
			
			SearchDisplayController.SearchResultsTableView.RowHeight = FlightTableViewCell.CellHeight;
			SearchDisplayController.SearchResultsTableView.AllowsSelection = true;
			
			TableView.RowHeight = FlightTableViewCell.CellHeight;
			TableView.AllowsSelection = true;
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			
			if (selected == null) {
				// Try to select the first aircraft. If that fails, add a new one.
				//selected = NSIndexPath.FromRowSection (0, 0);
				//SelectOrAdd (0);
			}
		}
		
		protected override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath, Flight flight)
		{
			FlightTableViewCell cell = tableView.DequeueReusableCell (key) as FlightTableViewCell;
			
			if (cell == null)
				cell = new FlightTableViewCell (key);
			
			cell.Flight = flight;
			
			return cell;
		}
		
		protected override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{
			return true;
		}
		
		protected override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return UITableViewCellEditingStyle.Delete;
		}
		
		protected override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			if (editingStyle != UITableViewCellEditingStyle.Delete)
				return;
			
			Flight flight = GetItem (tableView, indexPath);
			
			LogBook.Delete (flight);
		}
		
		static bool PathsEqual (NSIndexPath path0, NSIndexPath path1)
		{
			return path0.Section == path1.Section && path0.Row == path1.Row;
		}
		
		protected override NSIndexPath WillSelectRow (UITableView tableView, NSIndexPath indexPath)
		{
			if (tableView == SearchDisplayController.SearchResultsTableView) {
				if (searchSelected != null && !PathsEqual (searchSelected, indexPath))
					tableView.DeselectRow (searchSelected, false);
			} else {
				if (selected != null && !PathsEqual (selected, indexPath))
					tableView.DeselectRow (selected, false);
			}
			
			return indexPath;
		}
		
		protected override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			details.Flight = GetItem (tableView, indexPath);
			
			if (tableView == SearchDisplayController.SearchResultsTableView) {
				searchSelected = indexPath;
			} else {
				selected = indexPath;
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			LogBook.FlightAdded -= OnFlightAdded;
			LogBook.FlightUpdated -= OnFlightUpdated;
			LogBook.FlightDeleted -= OnFlightDeleted;
		}
	}
}
