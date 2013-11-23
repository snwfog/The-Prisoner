﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Cold_Ship
{
    public class Character : GenericSprite2D
    {
        //declare member variables
        public Vector2 prevPosition;
        public Vector2 velocity;
        public double bodyTemperature;
        public double stamina;
        public double staminaLimit;

        //animation related variables
        public enum Action_Status { FOWARD = 0, BACKWARD = 1, FORWARD_WITH_LIGHTER = 2, BACKWARD_WITH_LIGHTER = 3, CLIMB = 4 };
        public Action_Status actionStatus;
        public int maxFramesX, maxFramesY, currentFrame;
        public float animationTimer = 150;
        public Vector2 playerSpriteSize; //for collision once the animation is set up

        //internal member variables
        float normalTempDecreaseRate = -0.01f;
        float exertForceIncreaseRate = 0.002f;
        float exertForceDecreaseRate = -0.05f;
        bool isExertingForce = false;
        bool stoppedExertingForce = false;
        public bool isjumping = false;
        bool isClimbing = false;
        bool canClimb = false;

        //declare constructor for inheritance
        public Character(Texture2D texture, Vector2 position) : base(texture, position, Rectangle.Empty)
        {
            velocity = new Vector2(0, 0);
            this.bodyTemperature = 36;
        }

        //declare constructor for player sprite
        public Character(Texture2D texture, Vector2 position, double bodyTemperature, double stamina, double staminaLimit, int maxFramesX, int maxFramesY) : base(texture, position, Rectangle.Empty)
        {
            this.texture = texture;
            this.position = position;
            velocity = new Vector2(0, 0);
            this.bodyTemperature = bodyTemperature;
            this.stamina = stamina;
            this.staminaLimit = staminaLimit;

            this.maxFramesX = maxFramesX;
            this.maxFramesY = maxFramesY;
            currentFrame = 0;
            actionStatus = Action_Status.FOWARD;
            animationTimer = 0;
            playerSpriteSize = new Vector2((float)texture.Width / maxFramesX, (float)texture.Height / maxFramesY);
        }

        //draws the player sprite onto screen
        public override void Draw(SpriteBatch spriteBatch, Vector2 drawPosition)
        {
            int line = (int)actionStatus;
            Rectangle rect = new Rectangle(currentFrame * (int)playerSpriteSize.X, line * (int)playerSpriteSize.Y, (int)playerSpriteSize.X, (int)playerSpriteSize.Y);
            spriteBatch.Draw(texture, drawPosition, rect, Color.White);
        }

        //move the sprite
        public void Move()
        {
            position += velocity;
        }

        //update everything about the Scene2DNode object
        public void Update(bool lighterAcquired, GameTime gameTime, ref float bodyTempTimer, ref float exhaustionTimer, ref KeyboardState oldKeyboardState, ref float jumpTimer, float ground, List<Platform> platforms, List<Ladder> ladders, Vector2 worldSize, ref float staminaExhaustionTimer)
        {
            //register the position before updating (prevPosition)
            prevPosition = position;
            //update timers
            float elapsedTime = gameTime.ElapsedGameTime.Milliseconds;
            bodyTempTimer += elapsedTime;
            exhaustionTimer += elapsedTime;
            jumpTimer += elapsedTime;
            staminaExhaustionTimer += elapsedTime;
            animationTimer += elapsedTime;

            //register keyboard inputs
            KeyboardState newKeyboardState = Keyboard.GetState();

            canClimb = false;
            if (ladders != null)
            {
                foreach (Ladder ladder in ladders)
                {
                    if (ladder.Update(this))
                        canClimb = true;
                }
            }
            if (!canClimb)
                isClimbing = false;

            UpdateKeyboard(lighterAcquired, oldKeyboardState, newKeyboardState, ref jumpTimer, ref animationTimer);

            //Move();
            oldKeyboardState = newKeyboardState;

            //detect platform collision
            //bool jumpable = false;
            foreach (Platform platform in platforms)
            {
                if (!platform.Update(this, prevPosition, jumpTimer, ground, isjumping))
                {
                    isjumping = false;
                }
            }

            //detect world boundary collision
            if (position.X < 0 || position.X + playerSpriteSize.X > worldSize.X)
            {
                position = prevPosition;
            }

            //update body temperature
            updateBodyTemperature(ref bodyTempTimer, ref exhaustionTimer);

            //recover stamina
            if (staminaExhaustionTimer > 1500)
            {
                if (stamina < staminaLimit)
                {
                    stamina += 0.04;
                }
            }
            if (stamina < 0)
            {
                stamina = 0;

                staminaExhaustionTimer = 700;
            }
            else if (stamina > staminaLimit + 5)
            {
                //staminaLimit = stamina;
            }
            else if (stamina > staminaLimit && stamina < staminaLimit + 5)
            {
                staminaExhaustionTimer = 0;
            }
            else if (stamina > staminaLimit)
            {
                stamina = staminaLimit;
            }

            //apply gravity
            prevPosition = position;
            if (!isClimbing)
            {
                Move();
                if (position.Y < ground - playerSpriteSize.Y && jumpTimer > 200)
                {
                    velocity = new Vector2(0, 5);

                }
                else if (position.Y > ground - playerSpriteSize.Y)
                {
                    isjumping = false;
                    position.Y = ground - playerSpriteSize.Y;
                }

            }
            else
            {
                if (position.Y > ground - playerSpriteSize.Y)
                {
                    isClimbing = false;
                    position.Y = ground - playerSpriteSize.Y;
                }
            }
            foreach (Platform platform in platforms)
            {
                if (!platform.Update(this, prevPosition, jumpTimer, ground, isjumping))
                {
                    isjumping = false;
                }
            }
        }

        //update the sprite position based on the keyboard inputs
        public void UpdateKeyboard(bool lighterAcquired,KeyboardState oldKeyboardState, KeyboardState newKeyboardState, ref float jumpTimer, ref float animationTimer)
        {
            Keys[] keys = newKeyboardState.GetPressedKeys();
            foreach (Keys key in keys)
            {

                    if(key == HelperFunction.KeyLeft)
                    {
                        if (/*oldKeyboardState.IsKeyDown(Keys.LeftShift) &&*/ newKeyboardState.IsKeyDown(HelperFunction.KeySpeed) && stamina != 0)
                        {
                            isExertingForce = true;
                            stoppedExertingForce = false;
                            position += new Vector2(-5, 0);
                            stamina -= 0.2;

                            if (!lighterAcquired)
                            {
                                if (actionStatus != Action_Status.BACKWARD)
                                {
                                    actionStatus = Action_Status.BACKWARD;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 75 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= maxFramesX)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }
                            }
                            else if (lighterAcquired)
                            {
                                if (actionStatus != Action_Status.BACKWARD_WITH_LIGHTER)
                                {
                                    actionStatus = Action_Status.BACKWARD_WITH_LIGHTER;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 75 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= maxFramesX)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }
                            }

                        }
                        else if (oldKeyboardState.IsKeyDown(HelperFunction.KeySpeed) && newKeyboardState.IsKeyUp(HelperFunction.KeySpeed))
                        {
                            isExertingForce = false;
                            stoppedExertingForce = true;
                            position += new Vector2(-3, 0);
                            stamina -= 0.03;
                        }
                        else
                        {
                            isExertingForce = false;
                            //stoppedExertingForce = false;
                            position += new Vector2(-3, 0);
                            stamina -= 0.03;

                            if (!lighterAcquired)
                            {
                                if (actionStatus != Action_Status.BACKWARD)
                                {
                                    actionStatus = Action_Status.BACKWARD;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 150 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= maxFramesX)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }
                            }
                            else if (lighterAcquired)
                            {
                                if (actionStatus != Action_Status.BACKWARD_WITH_LIGHTER)
                                {
                                    actionStatus = Action_Status.BACKWARD_WITH_LIGHTER;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 150 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= maxFramesX)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }
                            }

                        }
                    }
                    else if (key == HelperFunction.KeyRight)
                    {
                        if (/*oldKeyboardState.IsKeyDown(Keys.LeftShift) &&*/ newKeyboardState.IsKeyDown(HelperFunction.KeySpeed) && stamina != 0)
                        {
                            isExertingForce = true;
                            stoppedExertingForce = false;
                            position += new Vector2(5, 0);
                            stamina -= 0.2;

                            if (!lighterAcquired)
                            {
                                if (actionStatus != Action_Status.FOWARD)
                                {
                                    actionStatus = Action_Status.FOWARD;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 75 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= maxFramesX)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }
                            }
                            else if (lighterAcquired)
                            {
                                if (actionStatus != Action_Status.FORWARD_WITH_LIGHTER)
                                {
                                    actionStatus = Action_Status.FORWARD_WITH_LIGHTER;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 75 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= maxFramesX)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }
                            }

                        }
                        else if (oldKeyboardState.IsKeyDown(HelperFunction.KeySpeed) && newKeyboardState.IsKeyUp(HelperFunction.KeySpeed))
                        {
                            isExertingForce = false;
                            stoppedExertingForce = true;
                            position += new Vector2(3, 0);
                            stamina -= 0.03;
                        }
                        else
                        {
                            isExertingForce = false;
                            //stoppedExertingForce = false;
                            position += new Vector2(3, 0);
                            stamina -= 0.03;

                            if (!lighterAcquired)
                            {
                                if (actionStatus != Action_Status.FOWARD)
                                {
                                    actionStatus = Action_Status.FOWARD;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 150 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= maxFramesX)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }
                            }
                            else if (lighterAcquired)
                            {
                                if (actionStatus != Action_Status.FORWARD_WITH_LIGHTER)
                                {
                                    actionStatus = Action_Status.FORWARD_WITH_LIGHTER;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 150 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= maxFramesX)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }
                            }
                        }
                    }
                    else if (key == HelperFunction.KeyUp)
                    {
                        if (canClimb)
                        {
                            isClimbing = true;
                            isjumping = false;
                            if (newKeyboardState.IsKeyDown(HelperFunction.KeySpeed) && stamina != 0)
                            {
                                isExertingForce = true;
                                stoppedExertingForce = false;
                                position += new Vector2(0, -5);
                                stamina -= 1;
                            }
                            else if (oldKeyboardState.IsKeyDown(HelperFunction.KeySpeed) && newKeyboardState.IsKeyUp(HelperFunction.KeySpeed))
                            {
                                isExertingForce = false;
                                stoppedExertingForce = true;
                                position += new Vector2(0, -3);
                                stamina -= 0.03;
                            }
                            else
                            {
                                isExertingForce = false;
                                //stoppedExertingForce = false;
                                position += new Vector2(0, -3);
                                stamina -= 0.03;

                                if (actionStatus != Action_Status.CLIMB)
                                {
                                    actionStatus = Action_Status.CLIMB;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 150 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= 2)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }

                            }
                        }
                    }
                    else if (key == HelperFunction.KeyDown)
                    {
                        if (canClimb)
                        {
                            isClimbing = true;
                            isjumping = false;
                            if (newKeyboardState.IsKeyDown(HelperFunction.KeySpeed) && stamina != 0)
                            {
                                isExertingForce = true;
                                stoppedExertingForce = false;
                                position += new Vector2(0, 5);
                                stamina -= 1;
                            }
                            else if (oldKeyboardState.IsKeyDown(HelperFunction.KeySpeed) && newKeyboardState.IsKeyUp(HelperFunction.KeySpeed))
                            {
                                isExertingForce = false;
                                stoppedExertingForce = true;
                                position += new Vector2(0, 3);
                                stamina -= 0.03;
                            }
                            else
                            {
                                isExertingForce = false;
                                //stoppedExertingForce = false;
                                position += new Vector2(0, 3);
                                stamina -= 0.03;

                                if (actionStatus != Action_Status.CLIMB)
                                {
                                    actionStatus = Action_Status.CLIMB;
                                    currentFrame = 0;
                                }
                                else if (animationTimer > 150 && !isjumping)
                                {
                                    currentFrame++;
                                    if (currentFrame >= 2)
                                    {
                                        currentFrame = 0;
                                    }
                                    animationTimer = 0;
                                }

                            }
                        }
                    }
                    else if (key == HelperFunction.KeyJump)
                    {
                        if (!isjumping && oldKeyboardState.IsKeyUp(Keys.Space) && stamina != 0)
                        {
                            position += new Vector2(0, -40);
                            velocity = new Vector2(0, -5);
                            isjumping = true;
                            jumpTimer = 0;
                            bodyTemperature -= 0.01;
                            stamina -= 0.5;
                            isClimbing = false;
                        }
                    }
            }
            if (keys.Length == 0)
            {
                currentFrame = 0;
            }
        }

        //update the body temperature
        public void updateBodyTemperature(ref float refbodyTempTimer, ref float refexhaustionTimer)
        {
            if (refbodyTempTimer > 500)
            {
                if (isExertingForce && !stoppedExertingForce)
                {
                    bodyTemperature += exertForceIncreaseRate;
                    refexhaustionTimer = 0;
                }
                else if (!isExertingForce && stoppedExertingForce && refexhaustionTimer < 3000)
                {
                    bodyTemperature += exertForceDecreaseRate;
                }
                else
                {
                    bodyTemperature += normalTempDecreaseRate;
                    //refexhaustionTimer = 0;
                }
                refbodyTempTimer = 0;

            }
        }

        //return player hitbox
        public Rectangle getPlayerHitBox()
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)playerSpriteSize.X, (int)playerSpriteSize.Y);
        }
    }
}