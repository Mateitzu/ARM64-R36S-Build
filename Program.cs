using Raylib_cs;
using System;
using System.Numerics;
// alias ca să nu ai conflicte
using RRectangle = Raylib_cs.Rectangle;

class Program
{
    // Modifică rezoluția dacă vrei (pe R36S recomand 320x240 sau 480x272)
    const int SCREEN_W = 640;
    const int SCREEN_H = 480;

    static void Main()
    {
        Raylib.InitWindow(SCREEN_W, SCREEN_H, "Pong - raylib-cs (bot)");
        Raylib.SetTargetFPS(60);

        RRectangle leftPaddle = new RRectangle(30, SCREEN_H / 2 - 40, 10, 80);
        RRectangle rightPaddle = new RRectangle(SCREEN_W - 40, SCREEN_H / 2 - 40, 10, 80);

        Vector2 ballPos = new Vector2(SCREEN_W / 2, SCREEN_H / 2);
        Vector2 ballVel = new Vector2(200, 150);

        int scoreL = 0, scoreR = 0;
        Random rng = new Random();

        // BOT settings
        float botMaxSpeed = 220f;   // px/sec - cât de repede poate mișca botul
        float botReaction = 0.9f;   // 0..1 - cât de „instant” urmărește bila (lower = mai greu)
        float botError = 0.0f;      // adaugă o eroare mică pentru dific. mică (px offset)

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            // --- Player INPUT (stânga)
            if (Raylib.IsKeyDown(KeyboardKey.W)) leftPaddle.Y -= 300f * dt;
            if (Raylib.IsKeyDown(KeyboardKey.S)) leftPaddle.Y += 300f * dt;

            // --- BOT (dreapta) — logic simplu: urmărește Y-ul bilei cu limitare de viteză
            {
                // punct țintă: poziția bilei + un mic offset predictive bazat pe direcția bilei
                float predictOffset = 0f;
                // dacă mingea se îndreaptă către bot, îl ajutăm cu mică predicție
                if (ballVel.X > 0)
                {
                    // predicție proporțională cu viteza verticală și distanța
                    float travel = (rightPaddle.X - ballPos.X) / ballVel.X;
                    predictOffset = ballVel.Y * travel * 0.2f; // factor mic
                }

                float targetY = ballPos.Y + predictOffset + (float)(rng.NextDouble() * 2 - 1) * botError;

                // distanță pe axa Y (punctul central al paletei)
                float paddleCenter = rightPaddle.Y + rightPaddle.Height / 2f;
                float diff = targetY - paddleCenter;

                // aplicăm reaction (smoother)
                float desiredVel = diff * 5f; // scalar pentru a obține o viteză țintă
                desiredVel = Math.Clamp(desiredVel, -botMaxSpeed, botMaxSpeed);

                // combina cu reaction factor (mai mic = mișcare mai „lenesa”)
                float move = desiredVel * botReaction;
                rightPaddle.Y += move * dt;
            }

            // clamp paddles in ecran
            leftPaddle.Y = Math.Clamp(leftPaddle.Y, 0f, SCREEN_H - leftPaddle.Height);
            rightPaddle.Y = Math.Clamp(rightPaddle.Y, 0f, SCREEN_H - rightPaddle.Height);

            // --- BALL physics
            ballPos.X += ballVel.X * dt;
            ballPos.Y += ballVel.Y * dt;

            // bounce top/bottom
            if (ballPos.Y <= 0)
            {
                ballPos.Y = 0;
                ballVel.Y *= -1;
            }
            else if (ballPos.Y >= SCREEN_H)
            {
                ballPos.Y = SCREEN_H;
                ballVel.Y *= -1;
            }

            // collisions cu palete
            RRectangle ballRect = new RRectangle(ballPos.X - 5, ballPos.Y - 5, 10, 10);
            if (Raylib.CheckCollisionRecs(ballRect, leftPaddle))
            {
                ballPos.X = leftPaddle.X + leftPaddle.Width + 5;
                ballVel.X = Math.Abs(ballVel.X) * 1.03f; // intoarce + usor accelereaza
                // adaugă variatie Y
                ballVel.Y += (float)(rng.NextDouble() * 80 - 40);
            }
            else if (Raylib.CheckCollisionRecs(ballRect, rightPaddle))
            {
                ballPos.X = rightPaddle.X - 5;
                ballVel.X = -Math.Abs(ballVel.X) * 1.03f;
                ballVel.Y += (float)(rng.NextDouble() * 80 - 40);
            }

            // scoring
            if (ballPos.X < -10)
            {
                scoreR++;
                ResetBall(ref ballPos, ref ballVel, rng, 1);
            }
            else if (ballPos.X > SCREEN_W + 10)
            {
                scoreL++;
                ResetBall(ref ballPos, ref ballVel, rng, -1);
            }

            // --- DRAW
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            // mid dashed line
            for (int y = 0; y < SCREEN_H; y += 20)
                Raylib.DrawLine(SCREEN_W / 2, y, SCREEN_W / 2, y + 10, Color.Gray);

            Raylib.DrawRectangleRec(leftPaddle, Color.White);
            Raylib.DrawRectangleRec(rightPaddle, Color.White);
            Raylib.DrawCircleV(ballPos, 5, Color.White);

            Raylib.DrawText(scoreL.ToString(), SCREEN_W / 2 - 60, 20, 40, Color.White);
            Raylib.DrawText(scoreR.ToString(), SCREEN_W / 2 + 30, 20, 40, Color.White);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static void ResetBall(ref Vector2 pos, ref Vector2 vel, Random rng, int dir)
    {
        pos = new Vector2(SCREEN_W / 2, SCREEN_H / 2);
        vel = new Vector2(200 * dir, rng.Next(-150, 150));
    }
}
