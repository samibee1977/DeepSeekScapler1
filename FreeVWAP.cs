#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Prop_Trader_Tools
{
	public class FreeVWAP : Indicator
	{
		

		private Series<double> sumPriceVolume;
		private Series<double> sumVolume;
		private Series<double> sumPPriceVVolume;
		private bool wapTrend;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"For more tools join -> https://discord.gg/gB75nGrzZx";
				Name										= "Free VWAP";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				ThisTool = "https://discord.gg/gB75nGrzZx";
				ShowStep1									= true;
				ShowStep2									= false;
				ShowStep3									= false;
				std1										= 0.382;
				std2										= 1.168;
				std3										= 2;
				
				SetStd1Color								= Brushes.Blue;
				SetStd1Opacity								= 20;
				
				SetStd2Color								= Brushes.Blue;
				SetStd2Opacity								= 10;
				
				SetStd3Color								= Brushes.Blue;
				SetStd3Opacity								= 5;
				
				ShowWeeklyWAP								= false;
				ShowOnlyWellklyWapMidLine					= false;
				RoundToTick									= false;
				ResetOnNewYorkOpenHour						= false;
				NYhour										=14;
				NYminute									=0;
				
				ShowHourWap 								= false;
				
				AddPlot(new Stroke(Brushes.DarkSlateGray, 4), PlotStyle.Line, "Vwap");
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "lowerWap");
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "upperWap");
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "lowerWap2");
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "upperWap2");
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "lowerWap3");
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "upperWap3");
				
				AddPlot(new Stroke(Brushes.Black, 5), PlotStyle.Line, "WeeklyWap");
				
				
				
			}
			else if (State == State.Configure)
			{
				
			}
			else if (State == State.DataLoaded)
			{	
				sumPriceVolume = new Series<double>(this);
				sumVolume = new Series<double>(this);
				sumPPriceVVolume = new Series<double>(this);
			}
			else if (State == State.Historical)
			{
				 SetZOrder(-1);
			}
			
			
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			
			if (CurrentBar < 1) return;
			
			if (Bars.IsFirstBarOfSession && !ShowWeeklyWAP)
      		{
				//takeTrades = true;
				sumPriceVolume[1]=0;
				sumVolume[1]=0;
				sumPPriceVVolume[1]=0;
			}
			if (Bars.IsFirstBarOfSession && ShowWeeklyWAP && Time[0].DayOfWeek == DayOfWeek.Sunday)
      		{
				//takeTrades = true;
				sumPriceVolume[1]=0;
				sumVolume[1]=0;
				sumPPriceVVolume[1]=0;
			}
			if (ResetOnNewYorkOpenHour && Time[0].TimeOfDay.Hours == NYhour && Time[0].TimeOfDay.Minutes == NYminute)
      		{
				//takeTrades = true;
				sumPriceVolume[1]=0;
				sumVolume[1]=0;
				sumPPriceVVolume[1]=0;
			}
			
			if (  ShowHourWap && !ResetOnNewYorkOpenHour && !ShowWeeklyWAP && ( 
				Time[0].TimeOfDay.Hours == 1 && Time[0].TimeOfDay.Minutes == 0 ||
				Time[0].TimeOfDay.Hours == 2 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 3 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 4 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 5 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 6 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 7 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 8 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 9 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 10 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 11 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 12 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 13 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 14 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 15 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 16 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 17 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 18 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 19 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 20 && Time[0].TimeOfDay.Minutes == 0 || 
				Time[0].TimeOfDay.Hours == 21 && Time[0].TimeOfDay.Minutes == 0 )

				)
      		{
				//takeTrades = true;
				sumPriceVolume[1]=0;
				sumVolume[1]=0;
				sumPPriceVVolume[1]=0;
				
				
			}
			
			
			double PriceT = (High[0]+Low[0]+Close[0] ) /3;
			
			sumPriceVolume[0] =  PriceT * Volume[0] + sumPriceVolume[1];
			sumVolume[0] = sumVolume[1]+ Volume[0];
			sumPPriceVVolume[0] = Volume[0] * Math.Pow(PriceT, 2) + sumPPriceVVolume[1];
			
			
			double toCalc = sumPriceVolume[0]/sumVolume[0];
			Value[0] = sumPriceVolume[0]/sumVolume[0];
			//if (IsRising(Value)) PlotBrushes[0][0] = Brushes.Lime;else PlotBrushes[0][0] = Brushes.Red;
			

			double variance = sumPPriceVVolume[0]/sumVolume[0];
			variance = variance - Math.Pow(Value[0],2);
			variance += variance < 0 ? 0 : variance;
			
			double stDev = Math.Sqrt(variance);
			
			if (ShowStep1){
			lowerWap[0] = Value[0] + stDev*std1;
			upperWap[0] = Value[0] - stDev*std1;
				Draw.Region(this,"zone1",CurrentBar, 0, lowerWap, upperWap, null,SetStd1Color, SetStd1Opacity);
			}
			if (ShowStep2){
			lowerWap2[0] = Value[0] + stDev*std2;
			upperWap2[0] = Value[0] - stDev*std2;
				Draw.Region(this,"zone2",CurrentBar, 0, lowerWap2, upperWap2, null,SetStd2Color, SetStd2Opacity);
			}
			if (ShowStep3){
			lowerWap3[0] = Value[0] + stDev*std3;
			upperWap3[0] = Value[0] - stDev*std3;	
				Draw.Region(this,"zone3",CurrentBar, 0, lowerWap3, upperWap3, null,SetStd3Color, SetStd3Opacity);
			}
			
			if ( RoundToTick )
				{
					lowerWap[0]=Instrument.MasterInstrument.RoundToTickSize(Value[0] + stDev*std1);
					upperWap[0]=Instrument.MasterInstrument.RoundToTickSize(Value[0] - stDev*std1);
					
					lowerWap2[0]=Instrument.MasterInstrument.RoundToTickSize(Value[0] + stDev*std2);
					upperWap2[0]=Instrument.MasterInstrument.RoundToTickSize(Value[0] - stDev*std2);
					
					lowerWap3[0]=Instrument.MasterInstrument.RoundToTickSize(Value[0] + stDev*std3);
					upperWap3[0]=Instrument.MasterInstrument.RoundToTickSize(Value[0] - stDev*std3);
					
					Draw.Region(this,"zone1",CurrentBar, 0, lowerWap, upperWap, null,SetStd1Color, SetStd1Opacity);
					Draw.Region(this,"zone2",CurrentBar, 0, lowerWap2, upperWap2, null,SetStd2Color, SetStd2Opacity);
					Draw.Region(this,"zone3",CurrentBar, 0, lowerWap3, upperWap3, null,SetStd3Color, SetStd3Opacity);
				}
			
			if ( RoundToTick )Value[0] = Instrument.MasterInstrument.RoundToTickSize(toCalc);
			if (Value[0] > Value[1]) wapTrend = true; 
			if (Value[0] < Value[1]) wapTrend = false; 
			
			
				
				
				if (wapTrend)PlotBrushes[0][0] = Brushes.Lime;else PlotBrushes[0][0] = Brushes.Red;
				
				//if ( Value[0] == Value[1] && Value[1] == Value[2] ) PlotBrushes[0][0] =  Brushes.Black;
			
			
		}

		#region Properties
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Support/HowToUse/SetUp -> :",  		       					Order=0, GroupName="0. Support")]
		public string ThisTool
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show Step 1", 				Order=1, GroupName="Parameters")]
		public bool ShowStep1
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Show Step 2", 				Order=2, GroupName="Parameters")]
		public bool ShowStep2
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Show Step 3", 				Order=3, GroupName="Parameters")]
		public bool ShowStep3
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Std Dev 1", 				Order=4, GroupName="Parameters")]
		public double std1
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Std Dev 2", 				Order=5, GroupName="Parameters")]
		public double std2
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Std Dev 3", 				Order=6, GroupName="Parameters")]
		public double std3
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SetStd1Color", 				Order=7, GroupName="Parameters")]
		public Brush SetStd1Color
		{ get; set; }

		[Browsable(false)]
		public string SetStd1ColorSerializable
		{
			get { return Serialize.BrushToString(SetStd1Color); }
			set { SetStd1Color = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SetStd1Opacity", 				Order=8, GroupName="Parameters")]
		public int SetStd1Opacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SetStd2Color", 				Order=9, GroupName="Parameters")]
		public Brush SetStd2Color
		{ get; set; }

		[Browsable(false)]
		public string SetStd2ColorSerializable
		{
			get { return Serialize.BrushToString(SetStd2Color); }
			set { SetStd2Color = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SetStd2Opacity", 				Order=10, GroupName="Parameters")]
		public int SetStd2Opacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SetStd3Color", 				Order=11, GroupName="Parameters")]
		public Brush SetStd3Color
		{ get; set; }

		[Browsable(false)]
		public string SetStd3ColorSerializable
		{
			get { return Serialize.BrushToString(SetStd3Color); }
			set { SetStd3Color = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SetStd3Opacity", 				Order=12, GroupName="Parameters")]
		public int SetStd3Opacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Weekly WAP", 				Order=13, GroupName="Parameters")]
		public bool ShowWeeklyWAP
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Weekly WAP Line Only", 				Order=14, GroupName="Parameters")]
		public bool ShowOnlyWellklyWapMidLine
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Round Lines to Tick Size", 				Order=15, GroupName="Parameters")]
		public bool RoundToTick
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Reset on Specific Session", 				Order=16, GroupName="Parameters")]
		public bool ResetOnNewYorkOpenHour
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="New York Open Hour", 				Order=17, GroupName="Parameters")]
		public int NYhour
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="New York Open Minute", 				Order=18, GroupName="Parameters")]
		public int NYminute
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Reset on every Hour", 				Order=19, GroupName="Parameters")]
		public bool ShowHourWap
		{ get; set; }
		
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Vwap
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> lowerWap
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> upperWap
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> lowerWap2
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> upperWap2
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> lowerWap3
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> upperWap3
		{
			get { return Values[6]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> WeeklyWap
		{
			get { return Values[7]; }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Prop_Trader_Tools.FreeVWAP[] cacheFreeVWAP;
		public Prop_Trader_Tools.FreeVWAP FreeVWAP(string thisTool, bool showStep1, bool showStep2, bool showStep3, double std1, double std2, double std3, Brush setStd1Color, int setStd1Opacity, Brush setStd2Color, int setStd2Opacity, Brush setStd3Color, int setStd3Opacity, bool showWeeklyWAP, bool showOnlyWellklyWapMidLine, bool roundToTick, bool resetOnNewYorkOpenHour, int nYhour, int nYminute, bool showHourWap)
		{
			return FreeVWAP(Input, thisTool, showStep1, showStep2, showStep3, std1, std2, std3, setStd1Color, setStd1Opacity, setStd2Color, setStd2Opacity, setStd3Color, setStd3Opacity, showWeeklyWAP, showOnlyWellklyWapMidLine, roundToTick, resetOnNewYorkOpenHour, nYhour, nYminute, showHourWap);
		}

		public Prop_Trader_Tools.FreeVWAP FreeVWAP(ISeries<double> input, string thisTool, bool showStep1, bool showStep2, bool showStep3, double std1, double std2, double std3, Brush setStd1Color, int setStd1Opacity, Brush setStd2Color, int setStd2Opacity, Brush setStd3Color, int setStd3Opacity, bool showWeeklyWAP, bool showOnlyWellklyWapMidLine, bool roundToTick, bool resetOnNewYorkOpenHour, int nYhour, int nYminute, bool showHourWap)
		{
			if (cacheFreeVWAP != null)
				for (int idx = 0; idx < cacheFreeVWAP.Length; idx++)
					if (cacheFreeVWAP[idx] != null && cacheFreeVWAP[idx].ThisTool == thisTool && cacheFreeVWAP[idx].ShowStep1 == showStep1 && cacheFreeVWAP[idx].ShowStep2 == showStep2 && cacheFreeVWAP[idx].ShowStep3 == showStep3 && cacheFreeVWAP[idx].std1 == std1 && cacheFreeVWAP[idx].std2 == std2 && cacheFreeVWAP[idx].std3 == std3 && cacheFreeVWAP[idx].SetStd1Color == setStd1Color && cacheFreeVWAP[idx].SetStd1Opacity == setStd1Opacity && cacheFreeVWAP[idx].SetStd2Color == setStd2Color && cacheFreeVWAP[idx].SetStd2Opacity == setStd2Opacity && cacheFreeVWAP[idx].SetStd3Color == setStd3Color && cacheFreeVWAP[idx].SetStd3Opacity == setStd3Opacity && cacheFreeVWAP[idx].ShowWeeklyWAP == showWeeklyWAP && cacheFreeVWAP[idx].ShowOnlyWellklyWapMidLine == showOnlyWellklyWapMidLine && cacheFreeVWAP[idx].RoundToTick == roundToTick && cacheFreeVWAP[idx].ResetOnNewYorkOpenHour == resetOnNewYorkOpenHour && cacheFreeVWAP[idx].NYhour == nYhour && cacheFreeVWAP[idx].NYminute == nYminute && cacheFreeVWAP[idx].ShowHourWap == showHourWap && cacheFreeVWAP[idx].EqualsInput(input))
						return cacheFreeVWAP[idx];
			return CacheIndicator<Prop_Trader_Tools.FreeVWAP>(new Prop_Trader_Tools.FreeVWAP(){ ThisTool = thisTool, ShowStep1 = showStep1, ShowStep2 = showStep2, ShowStep3 = showStep3, std1 = std1, std2 = std2, std3 = std3, SetStd1Color = setStd1Color, SetStd1Opacity = setStd1Opacity, SetStd2Color = setStd2Color, SetStd2Opacity = setStd2Opacity, SetStd3Color = setStd3Color, SetStd3Opacity = setStd3Opacity, ShowWeeklyWAP = showWeeklyWAP, ShowOnlyWellklyWapMidLine = showOnlyWellklyWapMidLine, RoundToTick = roundToTick, ResetOnNewYorkOpenHour = resetOnNewYorkOpenHour, NYhour = nYhour, NYminute = nYminute, ShowHourWap = showHourWap }, input, ref cacheFreeVWAP);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Prop_Trader_Tools.FreeVWAP FreeVWAP(string thisTool, bool showStep1, bool showStep2, bool showStep3, double std1, double std2, double std3, Brush setStd1Color, int setStd1Opacity, Brush setStd2Color, int setStd2Opacity, Brush setStd3Color, int setStd3Opacity, bool showWeeklyWAP, bool showOnlyWellklyWapMidLine, bool roundToTick, bool resetOnNewYorkOpenHour, int nYhour, int nYminute, bool showHourWap)
		{
			return indicator.FreeVWAP(Input, thisTool, showStep1, showStep2, showStep3, std1, std2, std3, setStd1Color, setStd1Opacity, setStd2Color, setStd2Opacity, setStd3Color, setStd3Opacity, showWeeklyWAP, showOnlyWellklyWapMidLine, roundToTick, resetOnNewYorkOpenHour, nYhour, nYminute, showHourWap);
		}

		public Indicators.Prop_Trader_Tools.FreeVWAP FreeVWAP(ISeries<double> input , string thisTool, bool showStep1, bool showStep2, bool showStep3, double std1, double std2, double std3, Brush setStd1Color, int setStd1Opacity, Brush setStd2Color, int setStd2Opacity, Brush setStd3Color, int setStd3Opacity, bool showWeeklyWAP, bool showOnlyWellklyWapMidLine, bool roundToTick, bool resetOnNewYorkOpenHour, int nYhour, int nYminute, bool showHourWap)
		{
			return indicator.FreeVWAP(input, thisTool, showStep1, showStep2, showStep3, std1, std2, std3, setStd1Color, setStd1Opacity, setStd2Color, setStd2Opacity, setStd3Color, setStd3Opacity, showWeeklyWAP, showOnlyWellklyWapMidLine, roundToTick, resetOnNewYorkOpenHour, nYhour, nYminute, showHourWap);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Prop_Trader_Tools.FreeVWAP FreeVWAP(string thisTool, bool showStep1, bool showStep2, bool showStep3, double std1, double std2, double std3, Brush setStd1Color, int setStd1Opacity, Brush setStd2Color, int setStd2Opacity, Brush setStd3Color, int setStd3Opacity, bool showWeeklyWAP, bool showOnlyWellklyWapMidLine, bool roundToTick, bool resetOnNewYorkOpenHour, int nYhour, int nYminute, bool showHourWap)
		{
			return indicator.FreeVWAP(Input, thisTool, showStep1, showStep2, showStep3, std1, std2, std3, setStd1Color, setStd1Opacity, setStd2Color, setStd2Opacity, setStd3Color, setStd3Opacity, showWeeklyWAP, showOnlyWellklyWapMidLine, roundToTick, resetOnNewYorkOpenHour, nYhour, nYminute, showHourWap);
		}

		public Indicators.Prop_Trader_Tools.FreeVWAP FreeVWAP(ISeries<double> input , string thisTool, bool showStep1, bool showStep2, bool showStep3, double std1, double std2, double std3, Brush setStd1Color, int setStd1Opacity, Brush setStd2Color, int setStd2Opacity, Brush setStd3Color, int setStd3Opacity, bool showWeeklyWAP, bool showOnlyWellklyWapMidLine, bool roundToTick, bool resetOnNewYorkOpenHour, int nYhour, int nYminute, bool showHourWap)
		{
			return indicator.FreeVWAP(input, thisTool, showStep1, showStep2, showStep3, std1, std2, std3, setStd1Color, setStd1Opacity, setStd2Color, setStd2Opacity, setStd3Color, setStd3Opacity, showWeeklyWAP, showOnlyWellklyWapMidLine, roundToTick, resetOnNewYorkOpenHour, nYhour, nYminute, showHourWap);
		}
	}
}

#endregion
