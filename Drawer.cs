using static SDL2.SDL;

public static class Drawer
{
    public static void DrawDot(this RenderArgs args, int x, int y, SDL_Color color)
    {
        SDL_SetRenderDrawColor(args.RendererPtr, color.r, color.g, color.b, color.a);
        SDL_RenderDrawPoint(args.RendererPtr, x, y);

        SDL_RenderDrawPoint(args.RendererPtr, x + 1, y);
        SDL_RenderDrawPoint(args.RendererPtr, x, y + 1);
        SDL_RenderDrawPoint(args.RendererPtr, x + 1, y + 1);

        SDL_RenderDrawPoint(args.RendererPtr, x - 1, y);
        SDL_RenderDrawPoint(args.RendererPtr, x, y - 1);
        SDL_RenderDrawPoint(args.RendererPtr, x - 1, y - 1);

    }

    public static void DrawCircle(this RenderArgs args, int centerX, int centerY, int radius, SDL_Color color)
    {
        SDL_SetRenderDrawColor(args.RendererPtr, color.r, color.g, color.b, color.a);

        int x = radius - 1;
        int y = 0;
        int tx = 1;
        int ty = 1;
        int err = tx - (radius << 1); // shifting bits left by 1 effectively
                                      // doubles the value. == tx - diameter

        while (x >= y)
        {
            SDL_RenderDrawPoint(args.RendererPtr, centerX + x, centerY - y);
            SDL_RenderDrawPoint(args.RendererPtr, centerX + x, centerY + y);
            SDL_RenderDrawPoint(args.RendererPtr, centerX - x, centerY - y);
            SDL_RenderDrawPoint(args.RendererPtr, centerX - x, centerY + y);
            SDL_RenderDrawPoint(args.RendererPtr, centerX + y, centerY - x);
            SDL_RenderDrawPoint(args.RendererPtr, centerX + y, centerY + x);
            SDL_RenderDrawPoint(args.RendererPtr, centerX - y, centerY - x);
            SDL_RenderDrawPoint(args.RendererPtr, centerX - y, centerY + x);

            if (err <= 0)
            {
                y++;
                err += ty;
                ty += 2;
            }

            if (err > 0)
            {
                x--;
                tx += 2;
                err += tx - (radius << 1);
            }
        }
    }

    public static SDL_Color ToColor(this BaseItentifier identifer) => identifer switch
    {
        BaseItentifier.Red => new SDL_Color() { r = 255, g = 25, b = 25, a = 255 },
        BaseItentifier.Blue => new SDL_Color() { r = 25, g = 25, b = 255, a = 255 },
        BaseItentifier.Green => new SDL_Color() { r = 25, g = 255, b = 25, a = 255 },
        BaseItentifier.Yellow => new SDL_Color() { r = 255, g = 255, b = 25, a = 255 },
        _ => throw new NotImplementedException()
    };



    internal static void DrawLine(RenderArgs args, int x1, int y1, int x2, int y2, BaseItentifier color)
    {
        var c = color.ToColor();
        SDL_SetRenderDrawColor(args.RendererPtr, c.r, c.g, c.b, c.a);
        SDL_RenderDrawLine(args.RendererPtr, x1, y1, x2, y2);
    }
}

