using System.Diagnostics;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

public class SDLRenderer
{
    private IntPtr WindowPtr = IntPtr.Zero;
    private IntPtr RendererPtr = IntPtr.Zero;
    private IntPtr FontPtr = IntPtr.Zero;

    private int ScreenWidth = 320;
    private int ScreenHeight = 240;

    private bool Running = true;
    private int Fps = 0;

    private Action<SDL_Event> _eventHandler;
    private Action<RenderArgs> _renderHandler;



    public SDLRenderer
    (
        Action<SDL_Event> eventHandler,
        Action<RenderArgs> renderHandler,
        int width = 320, int height = 240
    )
    {
        ScreenWidth = width;
        ScreenHeight = height;
        _eventHandler = eventHandler;
        _renderHandler = renderHandler;
        if (!SetupSdl()) throw new Exception("Failed to setup SDL");

    }

    public void Run(int targetFps = 60)
    {
        long lastTime = SDL_GetTicks();
        long currentTime = SDL_GetTicks();
        long deltaTime;

        while (Running)
        {
            currentTime = SDL_GetTicks();
            deltaTime = currentTime - lastTime;
            if (targetFps > 0)
            {

                if (deltaTime < 1000 / targetFps)
                {

                    var timeToSleep = (1000 / targetFps) - deltaTime;
                    Thread.Sleep((int)timeToSleep);
                    continue;
                }

                lastTime = currentTime;
                Fps = (int)(1000 / deltaTime);
            }

            if (deltaTime > 1000)
            {
                // If we hit a breakpoint, the delta time will be huge.
                continue;
            }

            HandleEvents();
            Render(deltaTime);
        }
    }

    internal void Dispose()
    {
        SDL_DestroyRenderer(RendererPtr);
        SDL_DestroyWindow(WindowPtr);
        TTF_CloseFont(FontPtr);
        TTF_Quit();
        SDL_Quit();
    }

    private void HandleEvents()
    {
        SDL_Event e;
        while (SDL_PollEvent(out e) != 0)
        {
            switch (e.type)
            {
                case SDL_EventType.SDL_QUIT:
                    Running = false;
                    break;
                default:
                    _eventHandler?.Invoke(e);
                    break;
            }
        }
    }

    private void Render(double deltaTime)
    {
        SDL_SetRenderDrawColor(RendererPtr, 0x10, 0x10, 0x00, 0xFF);
        SDL_RenderClear(RendererPtr);
        _renderHandler?.Invoke(new RenderArgs(WindowPtr, RendererPtr, FontPtr, Fps, deltaTime, ScreenWidth, ScreenHeight));
        RenderFps();
        SDL_RenderPresent(RendererPtr);
    }

    private int[] _fpsHistory = new int[60];
    private int _fpsHistoryIndex = 0;
    private void RenderFps()
    {
        _fpsHistory[_fpsHistoryIndex] = Fps;
        _fpsHistoryIndex++;
        if (_fpsHistoryIndex >= _fpsHistory.Length)
        {
            _fpsHistoryIndex = 0;
        }

        var avgFps = _fpsHistory.Average();

        var fpsText = $"FPS: {avgFps:00}";
        var FPSSurface = SDL2.SDL_ttf.TTF_RenderText_Solid(FontPtr, fpsText,
        new SDL_Color() { r = 0xFF, g = 0xFF, b = 0xFF, a = 0xFF });
        var fpsTexture = SDL_CreateTextureFromSurface(RendererPtr, FPSSurface);
        SDL_Rect fpsRect = new SDL_Rect()
        {
            x = 0,
            y = 0,
            w = 100 / 2,
            h = 24 / 2
        };
        SDL_RenderCopy(RendererPtr, fpsTexture, IntPtr.Zero, ref fpsRect);
        SDL_DestroyTexture(fpsTexture);
        SDL_FreeSurface(FPSSurface);
    }

    private bool SetupSdl()
    {
        if (SDL_Init(SDL_INIT_VIDEO) < 0)
        {
            Console.WriteLine($"SDL could not initialize! SDL_Error: {SDL_GetError()}");
            return false;
        }

        SDL_GetVersion(out var ver);


        Console.WriteLine($"SDL V{ver.major}.{ver.minor}.{ver.patch} initialized");

        var res = SDL_CreateWindowAndRenderer(ScreenWidth, ScreenHeight,
            SDL_WindowFlags.SDL_WINDOW_SHOWN,
            out WindowPtr,
            out RendererPtr);

        if (res < 0)
        {
            Console.WriteLine($"SDL could not create window and renderer! SDL_Error: {SDL_GetError()}");
            return false;
        }

        Debug.Assert(WindowPtr != IntPtr.Zero);
        Debug.Assert(RendererPtr != IntPtr.Zero);

        if (SDL2.SDL_ttf.TTF_Init() < 0)
        {
            Console.WriteLine($"SDL_ttf could not initialize! SDL_ttf Error:" +
            $"{SDL2.SDL_ttf.TTF_GetError()}");
            return false;
        }

        FontPtr = SDL2.SDL_ttf.TTF_OpenFont("Assets/TerminusTTF.ttf", 12);
        if (FontPtr == IntPtr.Zero)
        {
            Console.WriteLine($"Failed to load font! SDL_ttf Error: {SDL2.SDL_ttf.TTF_GetError()}");
            return false;
        }


        return true;
    }
}


public struct RenderArgs
{
    public IntPtr WindowPtr;
    public IntPtr RendererPtr;
    public IntPtr FontPtr;
    public int Fps;
    public double DeltaTime;
    public int ScreenWidth_Px;
    public int ScreenHeight_px;
    public RenderArgs(IntPtr windowPtr, nint rendererPtr, nint fontPtr, int fps, double deltaTime, int width_px, int height_px)
    {
        RendererPtr = rendererPtr;
        FontPtr = fontPtr;
        Fps = fps;
        DeltaTime = deltaTime;
        WindowPtr = windowPtr;
        ScreenWidth_Px = width_px;
        ScreenHeight_px = height_px;
    }

}