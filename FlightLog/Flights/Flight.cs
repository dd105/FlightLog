// 
// LogBookEntry.cs
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

using MonoTouch.SQLite;

namespace FlightLog {
	public enum FlightProperty {
		Date,
		Aircraft,
		[HumanReadableName ("Departed")]
		AirportDeparted,
		[HumanReadableName ("Visited")]
		AirportVisited,
		[HumanReadableName ("Arrived")]
		AirportArrived,

		[HumanReadableName ("Flight Time")]
		FlightTime,
		[HumanReadableName ("Day Flying")]
		Day,
		[HumanReadableName ("Night Flying")]
		Night,
		[HumanReadableName ("Cross-Country")]
		CrossCountry,
		[HumanReadableName ("Cross-Country (PIC)")]
		CrossCountryPIC,
		[HumanReadableName ("Certified Flight Instructor")]
		CertifiedFlightInstructor,
		[HumanReadableName ("Dual Received")]
		DualReceived,
		[HumanReadableName ("Pilot In Command")]
		PilotInCommand,
		[HumanReadableName ("Second In Command")]
		SecondInCommand,
		[HumanReadableName ("Day Landings")]
		DayLandings,
		[HumanReadableName ("Night Landings")]
		NightLandings,

		[HumanReadableName ("Actual Time")]
		InstrumentActual,
		[HumanReadableName ("Hood Time")]
		InstrumentHood,
		[HumanReadableName ("Simulator Time")]
		InstrumentSimulator,
		[HumanReadableName ("Approaches")]
		InstrumentApproaches,
		[HumanReadableName ("Holding Procedures")]
		InstrumentHoldingProcedures,
		[HumanReadableName ("Safety Pilot")]
		InstrumentSafetyPilot,

		Remarks,
	}

	public class Flight {
		public Flight (DateTime date)
		{
			Date = date;
		}
		
		public Flight ()
		{
		}
		
		/// <summary>
		/// Gets or sets the LogBook entry identifier.
		/// 
		/// Note: This value is set by the LogBook on insertion.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[PrimaryKey][AutoIncrement]
		public int Id {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the date of the flight.
		/// </summary>
		/// <value>
		/// The date.
		/// </value>
		public DateTime Date {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the tail number of the aircraft flown.
		/// </summary>
		/// <value>
		/// The aircraft's tail number.
		/// </value>
		[Indexed][MaxLength (9)][SQLiteSearchAlias ("tail")]
		public string Aircraft {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the FAA code of the airport departed.
		/// </summary>
		/// <value>
		/// The FAA code of the airport departed.
		/// </value>
		[Indexed][MaxLength (4)][SQLiteSearchAlias ("departed")]
		public string AirportDeparted {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the FAA code of the arrival airport.
		/// </summary>
		/// <value>
		/// The FAA code of the arrival airport.
		/// </value>
		[Indexed][MaxLength (4)][SQLiteSearchAlias ("arrived")]
		public string AirportArrived {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the FAA code of the visited airport(s).
		/// </summary>
		/// <value>
		/// The FAA codes of the visited airports.
		/// </value>
		[Indexed][MaxLength (140)][SQLiteSearchAlias ("visited")][SQLiteSearchAlias ("via")]
		public string AirportVisited {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the number of day landings.
		/// </summary>
		/// <value>
		/// The number of day landings.
		/// </value>
		public int DayLandings {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the number of night landings.
		/// </summary>
		/// <value>
		/// The number of night landings.
		/// </value>
		public int NightLandings {
			get; set;
		}
		
		#region Flight Time
		/// <summary>
		/// Gets or sets the total flight time.
		/// </summary>
		/// <value>
		/// The total flight time, in seconds.
		/// </value>
		public int FlightTime {
			get; set;
		}

		/// <summary>
		/// Gets or sets the time spent flying during daylight hours.
		/// </summary>
		/// <value>
		/// The time spent flying during daylight hours, in seconds.
		/// </value>
		public int Day {
			get; set;
		}

		/// <summary>
		/// Gets or sets the time spent flying during the night.
		/// </summary>
		/// <value>
		/// The time spent flying during the night, in seconds.
		/// </value>
		public int Night {
			get; set;
		}

		/// <summary>
		/// Gets or sets the time spent flying cross-country.
		/// </summary>
		/// <value>
		/// The time spent flying cross-country, in seconds.
		/// </value>
		[SQLiteSearchAlias ("xc")]
		public int CrossCountry {
			get; set;
		}

		/// <summary>
		/// Gets or sets the time spent flying as certified flight instructor.
		/// </summary>
		/// <value>
		/// The time spent flying as certified flight instructor, in seconds.
		/// </value>
		[SQLiteSearchAlias ("cfi")]
		public int CertifiedFlightInstructor {
			get; set;
		}

		/// <summary>
		/// Gets or sets the dual time received (for student pilots).
		/// </summary>
		/// <value>
		/// The dual time received, in seconds.
		/// </value>
		[SQLiteSearchAlias ("dual")]
		public int DualReceived {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the time spent flying as "Pilot in Command" or Solo (for student pilots).
		/// </summary>
		/// <value>
		/// The time spent flying as pilot in command, in seconds.
		/// </value>
		[SQLiteSearchAlias ("pic")]
		public int PilotInCommand {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the time spent flying as "Second in Command".
		/// </summary>
		/// <value>
		/// The time spent flying as second in command, in seconds.
		/// </value>
		[SQLiteSearchAlias ("sic")]
		public int SecondInCommand {
			get; set;
		}
		#endregion
		
		#region Instrument Flight Time
		/// <summary>
		/// Gets or sets the number of IFR approaches.
		/// </summary>
		/// <value>
		/// The number of IFR approaches.
		/// </value>
		[SQLiteSearchAlias ("approaches")]
		public int InstrumentApproaches {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets whether or not the pilot performed holding procedures during any of the approaches.
		/// </summary>
		/// <value>
		/// <c>true</c> if holding procedures were performed; otherwise, <c>false</c>.
		/// </value>
		[SQLiteSearchAlias ("holds")]
		public bool InstrumentHoldingProcedures {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the time spent actually flying by instruments.
		/// </summary>
		/// <value>
		/// The time spent actually flying by instruments, in seconds.
		/// </value>
		[SQLiteSearchAlias ("ifr")]
		public int InstrumentActual {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the time spent flying IFR while under a hood.
		/// </summary>
		/// <value>
		/// The time spent flying while under a hood, in seconds.
		/// </value>
		[SQLiteSearchAlias ("hood")]
		public int InstrumentHood {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the time spent flying by instruments in a simulator.
		/// </summary>
		/// <value>
		/// The time spent flying by instruments in a simulator, in seconds.
		/// </value>
		[SQLiteSearchAlias ("sim")]
		public int InstrumentSimulator {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the name of the safety pilot.
		/// </summary>
		/// <value>
		/// The instrument safety pilot.
		/// </value>
		[Indexed][MaxLength (40)][SQLiteSearchAlias ("safety")]
		public string InstrumentSafetyPilot {
			get; set;
		}
		#endregion
		
		/// <summary>
		/// Gets or sets any remarks.
		/// </summary>
		/// <value>
		/// The remarks.
		/// </value>
		[MaxLength (140)]
		public string Remarks {
			get; set;
		}

		public int GetFlightTime (FlightProperty property)
		{
			switch (property) {
			case FlightProperty.FlightTime:
				return FlightTime;
			case FlightProperty.Day:
				return Day;
			case FlightProperty.Night:
				return Night;
			case FlightProperty.CrossCountry:
				return CrossCountry;
			case FlightProperty.CrossCountryPIC:
				return Math.Min (CrossCountry, PilotInCommand);
			case FlightProperty.CertifiedFlightInstructor:
				return CertifiedFlightInstructor;
			case FlightProperty.DualReceived:
				return DualReceived;
			case FlightProperty.PilotInCommand:
				return PilotInCommand;
			case FlightProperty.SecondInCommand:
				return SecondInCommand;
			case FlightProperty.InstrumentActual:
				return InstrumentActual;
			case FlightProperty.InstrumentHood:
				return InstrumentHood;
			case FlightProperty.InstrumentSimulator:
				return InstrumentSimulator;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
		
		/// <summary>
		/// Event that gets emitted when the Flight gets updated.
		/// </summary>
		public event EventHandler<EventArgs> Updated;
		
		internal void OnUpdated ()
		{
			var handler = Updated;
			
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
	}
}
