using System;
using System.Collections.Generic;
using CocosSharp;

namespace CocosSharpSudoku
{

    public class GameMenuLayer : CCLayerColor
    {
        public GameMenuLayer() : base(Common.color1)
        {
            Opacity = 240;
        }
        
        protected override void AddedToScene()
        {
            base.AddedToScene();

            var bounds = VisibleBoundsWorldspace;

            CCSprite logo = new CCSprite("logo");
            logo.Scale = 1.5f;
            logo.Position = new CCPoint(bounds.Size.Width / 2, (bounds.Size.Height / 6) * 5);
            AddChild(logo);

            CCSprite newGameSprite = new CCSprite("NewGameButton");
            CCSprite highScoresSprite = new CCSprite("HighScoresButton");
            CCSprite statisticsSprite = new CCSprite("StatisticsButton");

            CCMenuItem menuItemNewGame = new CCMenuItemImage(newGameSprite, newGameSprite, NewGameClicked);
            CCMenuItem menuItemHighScores = new CCMenuItemImage(highScoresSprite, highScoresSprite, HighScoresClicked);
            CCMenuItem menuItemStatistics = new CCMenuItemImage(statisticsSprite, statisticsSprite, StatisticsClicked);

            menuItemNewGame.Scale = 0.1f;
            menuItemHighScores.Scale = 0.1f;
            menuItemStatistics.Scale = 0.1f;

            var menu = new CCMenu(menuItemNewGame, menuItemHighScores, menuItemStatistics)
            {
                Position = new CCPoint(bounds.Size.Width / 2, bounds.Size.Height / 2),
            };
            menu.AlignItemsVertically(50);
            
            AddChild(menu);
        }

        private void NewGameClicked(object obj)
        {
            var game = GameLayer.GameScene(this.GameView);
            var transitionToGame = new CCTransitionMoveInR(0.3f, game);
            Director.RunWithScene(transitionToGame);
        }

        private void HighScoresClicked(object obj)
        {
            throw new NotImplementedException();
        }

        private void StatisticsClicked(object obj)
        {
            throw new NotImplementedException();
        }

        public static CCScene GameStartLayerScene(CCGameView mainWindow)
        {
            var scene = new CCScene(mainWindow);
            var layer = new GameMenuLayer();

            scene.AddChild(layer);

            return scene;
        }

    }
}