//
// FlightExtensions.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc.
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
using System.Collections.Generic;

namespace FlightLog
{
	public static class FlightExtension
	{
		internal static string FormatFlightTime (int seconds, bool force)
		{
			if (seconds == 0 && !force)
				return null;

			if (Settings.FlightTimeFormat == FlightTimeFormat.Decimal) {
				double time = Math.Round (seconds / 3600.0, 1);

				if (time > 0.9 && time < 1.1)
					return "1 hour";

				return time.ToString () + " hours";
			} else {
				int hh = seconds / 3600;
				int mm = (seconds % 3600) / 60;
				//int ss = seconds % 60;

				return string.Format ("{0}:{1}", hh.ToString ("00"), mm.ToString ("00"));
			}
		}

		public static string ToString (this Flight flight, FlightProperty property)
		{
			switch (property) {
			case FlightProperty.Date:
				return flight != null ? flight.Date.ToLongDateString () : string.Empty;
			case FlightProperty.Aircraft:
				return flight != null ? flight.Aircraft : string.Empty;
			case FlightProperty.AirportDeparted:
				return flight != null ? flight.AirportDeparted : string.Empty;
			case FlightProperty.AirportVisited:
				return flight != null ? flight.AirportVisited : null;
			case FlightProperty.AirportArrived:
				return flight != null ? flight.AirportArrived : string.Empty;
			case FlightProperty.FlightTime:
				return flight != null ? FormatFlightTime (flight.FlightTime, true) : string.Empty;
			case FlightProperty.CrossCountry:
				return flight != null ? FormatFlightTime (flight.CrossCountry, false) : null;
			case FlightProperty.CertifiedFlightInstructor:
				return flight != null ? FormatFlightTime (flight.CertifiedFlightInstructor, false) : null;
			case FlightProperty.DualReceived:
				return flight != null ? FormatFlightTime (flight.DualReceived, false) : null;
			case FlightProperty.PilotInCommand:
				return flight != null ? FormatFlightTime (flight.PilotInCommand, false) : null;
			case FlightProperty.SecondInCommand:
				return flight != null ? FormatFlightTime (flight.SecondInCommand, false) : null;
			case FlightProperty.Day:
				return flight != null ? FormatFlightTime (flight.Day, false) : null;
			case FlightProperty.Night:
				return flight != null ? FormatFlightTime (flight.Night, false) : null;
			case FlightProperty.DayLandings:
				return flight != null && flight.DayLandings > 0 ? flight.DayLandings.ToString () : null;
			case FlightProperty.NightLandings:
				return flight != null && flight.NightLandings > 0 ? flight.NightLandings.ToString () : null;
			case FlightProperty.InstrumentActual:
				return flight != null ? FormatFlightTime (flight.InstrumentActual, false) : null;
			case FlightProperty.InstrumentHood:
				return flight != null ? FormatFlightTime (flight.InstrumentHood, false) : null;
			case FlightProperty.InstrumentSimulator:
				return flight != null ? FormatFlightTime (flight.InstrumentSimulator, false) : null;
			case FlightProperty.InstrumentApproaches:
				return flight != null && flight.InstrumentApproaches > 0 ? flight.InstrumentApproaches.ToString () : null;
			case FlightProperty.InstrumentHoldingProcedures:
				return flight != null && flight.InstrumentHoldingProcedures ? "Yes" : null;
			case FlightProperty.InstrumentSafetyPilot:
				return flight != null && !string.IsNullOrEmpty (flight.InstrumentSafetyPilot) ? flight.InstrumentSafetyPilot : null;
			case FlightProperty.Remarks:
				return flight != null && !string.IsNullOrEmpty (flight.Remarks) ? flight.Remarks : null;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
	}
}
