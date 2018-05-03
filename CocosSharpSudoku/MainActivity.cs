using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using CocosSharp;

namespace CocosSharpSudoku
{
    [Activity(
        Label = "CocosSharpSudoku",
        AlwaysRetainTaskState = true,
        Icon = "@drawable/icon",
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
        ScreenOrientation = ScreenOrientation.Portrait,
        LaunchMode = LaunchMode.SingleInstance,
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our game view from the layout resource,
            // and attach the view created event to it
            CCGameView gameView = (CCGameView)FindViewById(Resource.Id.GameView);
            gameView.ViewCreated += LoadGame;
            //gameView.ViewCreated += LoadGameMenu;
        }

        void LoadGameMenu(object sender, EventArgs e)
        {
            CCGameView view = sender as CCGameView;
            view.ContentManager.RootDirectory = "Content";

            if (view != null)
            {
                var contentSearchPaths = new List<string>() { "Fonts", "Sounds", "Images" };
                CCSizeI viewSize = view.ViewSize;

                // Portrait mode
                int width = 720;
                int height = 1280;

                // Set world dimensions
                view.DesignResolution = new CCSizeI(width, height);
                view.ResolutionPolicy = CCViewResolutionPolicy.ShowAll;

                // Determine whether to use the high or low def versions of our images
                // Make sure the default texel to content size ratio is set correctly
                // Of course you're free to have a finer set of image resolutions e.g (ld, hd, super-hd)
                if (width < viewSize.Width)
                {
                    contentSearchPaths.Add("Images/Hd");
                    CCSprite.DefaultTexelToContentSizeRatio = 2.0f;
                }
                else
                {
                    contentSearchPaths.Add("Images/Ld");
                    CCSprite.DefaultTexelToContentSizeRatio = 1.0f;
                }

                view.ContentManager.SearchPaths = contentSearchPaths;

                CCScene gameMenu = GameMenuLayer.GameStartLayerScene(view);

                //CCScene menuScene = new CCScene(view);

                //menuScene.AddLayer(new GameMenuLayer());
                view.RunWithScene(gameMenu);
            }
        }

        void LoadGame(object sender, EventArgs e)
        {
            CCGameView gameView = sender as CCGameView;

            if (gameView != null)
            {
                var contentSearchPaths = new List<string>() { "Fonts", "Sounds", "Images" };
                CCSizeI viewSize = gameView.ViewSize;

                // Portrait mode
                int width = 720;
                int height = 1280;

                // Set world dimensions
                gameView.DesignResolution = new CCSizeI(width, height);
                gameView.ResolutionPolicy = CCViewResolutionPolicy.ShowAll;

                //// Determine whether to use the high or low def versions of our images
                //// Make sure the default texel to content size ratio is set correctly
                //// Of course you're free to have a finer set of image resolutions e.g (ld, hd, super-hd)
                //if (width < viewSize.Width)
                //{
                //    contentSearchPaths.Add("Images/Hd");
                //    CCSprite.DefaultTexelToContentSizeRatio = 2.0f;
                //}
                //else
                //{
                //    contentSearchPaths.Add("Images/Ld");
                //    CCSprite.DefaultTexelToContentSizeRatio = 1.0f;
                //}

                gameView.ContentManager.SearchPaths = contentSearchPaths;

                CCScene game = GameLayer.GameScene(gameView);
                gameView.RunWithScene(game);
                CCScene menu = GameMenuLayer.GameStartLayerScene(gameView);
                gameView.RunWithScene(menu);
            }
        }
    }
}

