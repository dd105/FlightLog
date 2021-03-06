// 
// EditAircraftDetailsViewController.cs
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
using System.Collections;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog
{
	public class EditAircraftDetailsViewController : DialogViewController
	{
		RadioGroup classes = new RadioGroup ("AircraftClassification", 0);
		BooleanElement isComplex, isHighPerformance, isTailDragger, isSimulator;
		RootElement category, classification;
		EditAircraftProfileView profile;
		UIBarButtonItem cancel, save;
		LimitedEntryElement notes;
		int previousCategory;
		bool exists;
		
		public EditAircraftDetailsViewController (Aircraft aircraft, bool exists) : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			this.exists = exists;
			Aircraft = aircraft;
			Autorotate = false;
		}
		
		public Aircraft Aircraft {
			get; private set;
		}
		
		public override UITableView MakeTableView (RectangleF bounds, UITableViewStyle style)
		{
			return base.MakeTableView (bounds, style);
		}

		static AircraftCategory CategoryFromIndex (int index)
		{
			return (AircraftCategory) (index * Aircraft.CategoryStep);
		}
		
		static int CategoryToIndex (AircraftCategory category)
		{
			return ((int) category) / Aircraft.CategoryStep;
		}
		
		static RootElement CreateCategoryElement (AircraftCategory category)
		{
			RootElement root = new RootElement ("Category", new RadioGroup ("AircraftCategory", 0));
			Section section = new Section ();
			
			foreach (AircraftCategory value in Enum.GetValues (typeof (AircraftCategory)))
				section.Add (new RadioElement (value.ToHumanReadableName (), "AircraftCategory"));
			
			root.Add (section);

			root.RadioSelected = CategoryToIndex (category);
			
			return root;
		}
		
		static AircraftClassification ClassificationFromIndexes (int category, int classification)
		{
			return (AircraftClassification) ((category * Aircraft.CategoryStep) + classification);
		}
		
		static int ClassificationToIndex (AircraftClassification classification)
		{
			return ((int) classification) % Aircraft.CategoryStep;
		}
		
		static RootElement CreateClassificationElement (AircraftCategory category, RadioGroup classes)
		{
			RootElement root = new RootElement ("Class", classes);
			int next = (int) category + Aircraft.CategoryStep;
			Section section = new Section ();
			
			foreach (AircraftClassification value in Enum.GetValues (typeof (AircraftClassification))) {
				if ((int) value < (int) category)
					continue;
				
				if ((int) value >= next)
					break;
				
				section.Add (new RadioElement (value.ToHumanReadableName (), "AircraftClassification"));
			}
			
			root.Add (section);
			
			return root;
		}
		
		Section CreateAircraftTypeSection ()
		{
			Section section = new Section ("Type of Aircraft") {
				(category = CreateCategoryElement (Aircraft.Category)),
				(classification = CreateClassificationElement (Aircraft.Category, classes)),
				(isComplex = new BooleanElement ("Complex", Aircraft.IsComplex)),
				(isHighPerformance = new BooleanElement ("High Performance", Aircraft.IsHighPerformance)),
				(isTailDragger = new BooleanElement ("Tail Dragger", Aircraft.IsTailDragger)),
				(isSimulator = new BooleanElement ("Simulator", Aircraft.IsSimulator)),
			};
			
			classification.RadioSelected = ClassificationToIndex (Aircraft.Classification);
			
			return section;
		}
		
		public override void LoadView ()
		{
			base.LoadView ();
			
			if (Aircraft == null) {
				Aircraft = new Aircraft ();
				exists = false;
			}
			
			Title = exists ? Aircraft.TailNumber : "New Aircraft";
			
			profile = new EditAircraftProfileView (View.Bounds.Width);
			profile.Photograph = PhotoManager.Load (Aircraft.TailNumber, false);
			profile.TailNumber = Aircraft.TailNumber;
			profile.Model = Aircraft.Model;
			profile.Make = Aircraft.Make;
			
			Root.Add (CreateAircraftTypeSection ());
			Root.Add (new Section ("Notes") {
				(notes = new LimitedEntryElement (null, "Enter any additional notes about the aircraft here.",
					Aircraft.Notes, 140)),
			});
			
			Root[0].HeaderView = profile;
			
			cancel = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, OnCancelClicked);
			NavigationItem.LeftBarButtonItem = cancel;
			
			save = new UIBarButtonItem (UIBarButtonSystemItem.Save, OnSaveClicked);
			NavigationItem.RightBarButtonItem = save;
		}
		
		void OnCancelClicked (object sender, EventArgs args)
		{
			NavigationController.PopViewControllerAnimated (true);
			
			OnEditorClosed ();
		}
		
		void OnSaveClicked (object sender, EventArgs args)
		{
			if (profile.TailNumber == null || profile.TailNumber.Length < 2)
				return;
			
			if (profile.Photograph != null) {
				UIImage thumbnail;
				NSError error;
				
				if (!PhotoManager.Save (profile.TailNumber, profile.Photograph, false, out error)) {
					UIAlertView alert = new UIAlertView ("Error", error.LocalizedDescription, null, "Dismiss", null);
					alert.Show ();
					return;
				}
				
				thumbnail = PhotoManager.ScaleToSize (profile.Photograph, 96, 72);
				PhotoManager.Save (profile.TailNumber, thumbnail, true, out error);
				// Note: we don't dispose the thumbnail because it has been added to the cache.
				thumbnail = null;
			}
			
			// Save the values back to the Aircraft object
			Aircraft.TailNumber = profile.TailNumber;
			Aircraft.Make = profile.Make;
			Aircraft.Model = profile.Model;
			Aircraft.Classification = ClassificationFromIndexes (category.RadioSelected, classes.Selected);
			Aircraft.IsComplex = isComplex.Value;
			Aircraft.IsHighPerformance = isHighPerformance.Value;
			Aircraft.IsTailDragger = isTailDragger.Value;
			Aircraft.IsSimulator = isSimulator.Value;
			Aircraft.Notes = notes.Value;
			
			// We'll treat this as a special case for now...
			if (Aircraft.Make == null && Aircraft.IsSimulator)
				Aircraft.Make = "Simulator";
			
			if (exists)
				LogBook.Update (Aircraft);
			else
				LogBook.Add (Aircraft);
			
			NavigationController.PopViewControllerAnimated (true);
			
			OnEditorClosed ();
		}
		
		public override void ViewWillAppear (bool animated)
		{
			if (category.RadioSelected != previousCategory) {
				var index = classification.IndexPath;
				
				classification = CreateClassificationElement (CategoryFromIndex (category.RadioSelected), classes);
				classification.RadioSelected = 0;
				
				Root[index.Section].Remove (index.Row);
				Root[index.Section].Insert (index.Row, classification);
				classes.Selected = 0;
				
				Root.Reload (Root[index.Section], UITableViewRowAnimation.Right);
				
				index.Dispose ();
			}
			
			base.ViewWillAppear (animated);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			previousCategory = category.RadioSelected;
			base.ViewWillDisappear (animated);
		}
		
		public event EventHandler<EventArgs> EditorClosed;
		
		void OnEditorClosed ()
		{
			var handler = EditorClosed;
			
			if (handler != null)
				handler (this, EventArgs.Empty);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
