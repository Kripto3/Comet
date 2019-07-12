﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HotUI {
	public static class HotReloadHelper {
		static HotReloadHelper()
		{
			IsEnabled = Debugger.IsAttached;
		}
		public static bool IsEnabled { get; set; }

		public static void Register(View view, params object[] parameters)
		{
			if (!IsEnabled)
				return;
			currentViews [view] = parameters;
		}

		public static void UnRegister(View view)
		{
			if (!IsEnabled)
				return;
			currentViews.Remove (view);
		}

		public static View GetReplacedView(View view)
		{
			if (!IsEnabled)
				return view;
			if (!replacedViews.TryGetValue (view.GetType ().FullName, out var newViewType))
				return view;

			currentViews.TryGetValue (view, out var parameters);
            try
            {
                var newView = parameters?.Length > 0 ? Activator.CreateInstance(newViewType, args: parameters) : Activator.CreateInstance(newViewType);
                //TODO: Apply state!
                return (View)newView;
            }
            catch(MissingMethodException ex)
            {
                Debug.WriteLine("You are using HotUI.Reload on a view that requires Parameters. Please call `HotReloadHelper.Register(this, params);` in the constructor;");
                throw ex;
            }

		}

		static Dictionary<string, Type> replacedViews = new Dictionary<string, Type> ();
		static Dictionary<View, object []> currentViews = new Dictionary<View, object []> ();
		public static void RegisterReplacedView(string oldViewType, Type newViewType)
		{
			if (!IsEnabled)
				return;
			replacedViews [oldViewType] = newViewType;
		}
		public static void TriggerReload()
		{
			var roots = View.ActiveViews.Where (x => x.Parent == null).ToList();
			foreach(var view in roots) {
				Device.InvokeOnMainThread (view.Reload);
			}
		}
	}
}