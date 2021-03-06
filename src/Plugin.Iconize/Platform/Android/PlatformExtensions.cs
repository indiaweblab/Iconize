﻿using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Plugin.Iconize
{
    /// <summary>
    /// Defines the <see cref="PlatformExtensions" /> extensions.
    /// </summary>
    public static class PlatformExtensions
    {
        private static readonly Dictionary<Type, Typeface> _fontCache = new Dictionary<Type, Typeface>();

        /// <summary>
        /// To the bitmap.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this Drawable drawable)
        {
            Bitmap bitmap = null;

            if (drawable is BitmapDrawable bitmapDrawable && bitmapDrawable.Bitmap != null)
            {
                return bitmapDrawable.Bitmap;
            }

            if (drawable.IntrinsicWidth <= 0 || drawable.IntrinsicHeight <= 0)
            {
                bitmap = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888); // Single color bitmap will be created of 1x1 pixel
            }
            else
            {
                bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            }

            var canvas = new Canvas(bitmap);
            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);
            return bitmap;
        }

        /// <summary>
        /// To the typeface.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static Typeface ToTypeface(this IIconModule module, Context context)
        {
            var moduleType = module.GetType();
            if (!_fontCache.ContainsKey(moduleType))
            {
                _fontCache.Add(moduleType, Typeface.CreateFromAsset(context.Assets, module.FontPath));
            }
            return _fontCache[moduleType];
        }

        /// <summary>
        /// Gets the toolbar item drawable.
        /// </summary>
        /// <param name="toolbarItem">The toolbar item.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Drawable GetToolbarItemDrawable(this ToolbarItem toolbarItem, Context context)
        {
            if (String.IsNullOrWhiteSpace(toolbarItem.Icon))
                return null;

            if (!(toolbarItem is IconToolbarItem iconItem))
                return context.GetDrawable(toolbarItem.Icon);

            var drawable = new IconDrawable(context, iconItem.Icon);
            if (drawable == null)
                return null;

            if (iconItem.IconColor != Xamarin.Forms.Color.Default)
                drawable = drawable.Color(iconItem.IconColor.ToAndroid());

            return drawable.ActionBarSize();
        }

        /// <summary>
        /// Updates the toolbar items.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="view">The view.</param>
        public static void UpdateToolbarItems(this Page page, Android.Views.View view)
        {
            var toolbar = view.FindViewById<Toolbar>(Iconize.ToolbarId);
            if (toolbar == null)
                return;

            toolbar.Menu.Clear();

            var toolbarItems = page.GetToolbarItems();
            if (toolbarItems == null)
                return;

            foreach (var toolbarItem in toolbarItems)
            {
                if (!((toolbarItem as IconToolbarItem)?.IsVisible ?? true))
                    continue;

                var menuItem = toolbar.Menu.Add(toolbarItem.Text);
                menuItem.SetOnMenuItemClickListener(new MenuClickListener(toolbarItem.Activate));

                var icon = toolbarItem.GetToolbarItemDrawable(toolbar.Context);
                if (icon != null)
                    menuItem.SetIcon(icon);

                if (toolbarItem.Order != ToolbarItemOrder.Secondary)
                    menuItem.SetShowAsAction(ShowAsAction.Always);
            }
        }
    }
}