#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using GamePad = Microsoft.Xna.Framework.Input.GamePad;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;
using KeyboardState = Microsoft.Xna.Framework.Input.KeyboardState;

#endregion

namespace Cold_Ship
{
  //declare the enum for game levels
  public enum Game_Level { LEVEL_HOLDING_CELL, LEVEL_PRISON_BLOCKS, LEVEL_GENERATOR, LEVEL_COMMON_ROOM, LEVEL_ENTERTAINMENT_ROOM };

  /// <summary>
  /// This is the main type for your game
  /// </summary>
  public class Cold_Ship : Game
  {
    public const bool DEBUG_MODE = true;
    public static DebugSprite DEBUG_TEXTURE;

    //declare needed global variables, commented out variables are no longer used
    public GraphicsDeviceManager graphics;
    public SpriteBatch SpriteBatch;
    //Scene2DNode playerNode, backgroundNode;
    public Vector2 WindowBound { get; set; }/*, worldSize*/
    public Texture2D DebugTexture { get; set; }

    public Camera2D Camera;
    // Freeze but display the regular screen
    // Used for dialogue
    // A few unique state that the game can be, these states are linear
    // Which means they cannot be combined togheter with one another
    // Only frozen is implemented so far, pause still needs work
    public enum GameState { FROZEN, DIALOGUING, PAUSED, ENDED, PLAYING, INTIALIZED, MENU, KEY_BINDING }
    private Stack<GameState> _gameState;
    // DIALOGUE USED COMPOENTS
    public List<DialogueBubble> DialogueQueue { get; set; }

    public Character Player;

    MainMenu mainMenu;
    PauseMenu pauseMenu;
    KeyBindingMenu keyBindingMenu;

    GameLevel BasicGameLevel;
    Level_Holding_Cell level0HoldingCell;

    Game_Level gameLevel = Game_Level.LEVEL_HOLDING_CELL;
    Game_Level prevGameLevel = Game_Level.LEVEL_HOLDING_CELL;

    public SpriteFont MonoSmall;
    public SpriteFont MonoMedium;


    double bodyTemperature = 36;
    double stamina = 100;
    double staminaLimit = 100;

    public Cold_Ship()
      : base()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      this._gameState = new Stack<GameState>();
      this._gameState.Push(GameState.INTIALIZED);
      this._gameState.Push(GameState.MENU);

      DEBUG_TEXTURE = new DebugSprite(this);
    }

    // Bunch of helper methods to deal with the state of the game at any moment
    public GameState GetCurrentGameState() { return _gameState.Peek(); }
    private void _setCurrentGameState(GameState state) { this._gameState.Push(state); }
    public void ActivateState(GameState state) { this._setCurrentGameState(state); }
    public GameState RestoreLastState() { return this._gameState.Pop(); }
    public bool GameStateIs(GameState state) { return this.GetCurrentGameState() == state; }


    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
      // TODO: Add your initialization logic here
      //initiate screen size
      WindowBound = new Vector2(800, 600);
      graphics.PreferredBackBufferWidth = (int)WindowBound.X;
      graphics.PreferredBackBufferHeight = (int)WindowBound.Y;
      //graphics.IsFullScreen = true;

      DialogueBubble.engine = new AudioEngine("Content\\Sounds\\SOUND_SPEECH_ENGINE.xgs");
      DialogueBubble.soundBank = new SoundBank(DialogueBubble.engine, "Content\\Sounds\\SOUND_SPEECH_SOUNDBANK.xsb");
      DialogueBubble.waveBank = new WaveBank(DialogueBubble.engine, "Content\\Sounds\\SOUND_SPEECH_WAVEBANK.xwb");

      MonoSmall = Content.Load<SpriteFont>("Fonts/Manaspace0");
      MonoMedium = Content.Load<SpriteFont>("Fonts/Manaspace12");

      // Create a new SpriteBatch, which can be used to draw textures.
      SpriteBatch = new SpriteBatch(GraphicsDevice);

      mainMenu = new MainMenu(this, Content.Load<Texture2D>("Textures\\platformTexture")
          , Content.Load<Texture2D>("Objects\\lighter"), Content.Load<SpriteFont>("Fonts\\manaspace12")
          , DialogueBubble.soundBank.GetCue("sound-next-char"));
      pauseMenu = new PauseMenu(this, Content.Load<Texture2D>("Textures\\platformTexture")
          , Content.Load<Texture2D>("Objects\\lighter"), Content.Load<SpriteFont>("Fonts\\manaspace12")
          , DialogueBubble.soundBank.GetCue("sound-next-char"));
      keyBindingMenu = new KeyBindingMenu(this, Content.Load<Texture2D>("Textures\\platformTexture")
          , Content.Load<Texture2D>("Objects\\lighter"), Content.Load<SpriteFont>("Fonts\\manaspace12")
          , DialogueBubble.soundBank.GetCue("sound-next-char"));

      this.Camera = new Camera2D(this);
      this.Player = Character.GetNewInstance(this, Camera);

      // DIALOGUE USED COMPONENT
      this.DialogueQueue = new List<DialogueBubble>();
      GameLevel _startingLevel = new Level_Holding_Cell(this, null, null);
      _startingLevel.NextGameLevel = new Level_Prison_Block(this, _startingLevel, null);
      this.Player.SpawnIn(_startingLevel);
      base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      //          level0HoldingCell.LoadContent();
      //            level1PrisonBlock.LoadContent();
      //            level2GeneratorRoom.LoadContent(Content, gameLevel, prevGameLevel, bodyTemperature, stamina, staminaLimit);
      //            level3CommonRoom.LoadContent(Content, gameLevel, prevGameLevel, bodyTemperature, stamina, staminaLimit);
      //            level4EntertainmentRoom.LoadContent(gameLevel, prevGameLevel, bodyTemperature, stamina, staminaLimit);
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// all content.
    /// </summary>
    protected override void UnloadContent()
    {
      //          level0HoldingCell.Unload();
      //level1PrisonBlock.Unload();
      //level2GeneratorRoom.Unload();
      //level3CommonRoom.Unload();
      //level4EntertainmentRoom.Unload();
    }

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
      if (Keyboard.GetState().IsKeyDown(Keys.Escape))
      {
        if (this.GameStateIs(GameState.PLAYING))
          ActivateState(GameState.PAUSED);
        if (DEBUG_MODE)
          this.Exit();
      }

      if (this.GameStateIs(GameState.PLAYING))
      {
        this.Player.Update(gameTime);
        switch (gameLevel)
        {
          //                    case Game_Level.LEVEL_PRISON_BLOCKS:
          //                        bodyTemperature = level1PrisonBlock.Update(gameTime, ref bodyTempTimer, ref exhaustionTimer, ref oldKeyboardState, ref jumpTimer, ref gameLevel, ref staminaExhaustionTimer, ref bodyTemperature, ref stamina, ref staminaLimit);
          //                        break;
          //                    case Game_Level.LEVEL_GENERATOR:
          //                        bodyTemperature = level2GeneratorRoom.Update(gameTime, ref bodyTempTimer, ref exhaustionTimer, ref oldKeyboardState, ref jumpTimer, ref gameLevel, ref staminaExhaustionTimer, ref bodyTemperature, ref stamina, ref staminaLimit);
          //                        break;
          //                    case Game_Level.LEVEL_HOLDING_CELL:
          //                        bodyTemperature = level0HoldingCell.Update(gameTime, ref bodyTempTimer, ref exhaustionTimer, ref oldKeyboardState, ref jumpTimer, ref gameLevel, ref staminaExhaustionTimer, ref bodyTemperature, ref stamina, ref staminaLimit);
          //                        break;
          //                    case Game_Level.LEVEL_COMMON_ROOM:
          //                        bodyTemperature = level3CommonRoom.Update(gameTime, ref bodyTempTimer, ref exhaustionTimer, ref oldKeyboardState, ref jumpTimer, ref gameLevel, ref staminaExhaustionTimer, ref bodyTemperature, ref stamina, ref staminaLimit);
          //                        break;
          //                    case Game_Level.LEVEL_ENTERTAINMENT_ROOM:
          //                        bodyTemperature = level4EntertainmentRoom.Update(gameTime, ref bodyTempTimer, ref exhaustionTimer, ref oldKeyboardState, ref jumpTimer, ref gameLevel, ref staminaExhaustionTimer, ref bodyTemperature, ref stamina, ref staminaLimit);
          //                        break;
        }

        if (prevGameLevel != gameLevel)
        {
          prevGameLevel = gameLevel;
        }
      }
      else if (this.GameStateIs(GameState.MENU))
      {
        mainMenu.Update(gameTime);
      }
      else if (this.GameStateIs(GameState.PAUSED))
      {
        pauseMenu.Update(gameTime);
      }
      else if (this.GameStateIs(GameState.KEY_BINDING))
      {
        keyBindingMenu.Update(gameTime);
      }

      base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// 
    //varaibles for capturing the fps
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.Black);

      if (this.GameStateIs(GameState.PLAYING) || this.GameStateIs(GameState.DIALOGUING))
      {
        //        Camera.DrawNode(this.Player);
        this.Player.DrawEnvironment(SpriteBatch);
        //                switch (gameLevel)
        //                {
        //                    case Game_Level.LEVEL_HOLDING_CELL:
        //                        level0HoldingCell.Draw();
        //                        break;
        ////                    case Game_Level.LEVEL_PRISON_BLOCKS:
        //                        level1PrisonBlock.Draw(framesPerSecond);
        //                        break;
        //                    case Game_Level.LEVEL_GENERATOR:
        //                        level2GeneratorRoom.Draw(framesPerSecond);
        //                        break;
        //                    case Game_Level.LEVEL_COMMON_ROOM:
        //                        level3CommonRoom.Draw(framesPerSecond);
        //                        break;
        //                    case Game_Level.LEVEL_ENTERTAINMENT_ROOM:
        //                        level4EntertainmentRoom.Draw(framesPerSecond);
        //                        break;
        //                }

        // Putting dialogue here cause they need to be appearing on top of everything
        if (this.GameStateIs(GameState.DIALOGUING))
        {
          SpriteBatch.Begin();
          foreach (DialogueBubble dialogue in this.DialogueQueue)
          {
            if (dialogue.IsPlaying())
            {
              dialogue.Draw(SpriteBatch);
              break;
            }
          }
          SpriteBatch.End();
        }
      }
      else if (this.GameStateIs(GameState.MENU))
      {
        mainMenu.Draw(SpriteBatch);
      }
      else if (this.GameStateIs(GameState.PAUSED))
      {
        pauseMenu.Draw(SpriteBatch);
      }
      else if (this.GameStateIs(GameState.KEY_BINDING))
      {
        keyBindingMenu.Draw(SpriteBatch);
      }

      base.Draw(gameTime);
    }
  }
}
