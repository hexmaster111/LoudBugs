using static SDL2.SDL;

internal class Program
{
    internal static int ScreenWidth = 500;
    internal static int ScreenHeight = 500;
    private static SDLRenderer _renderer = new SDLRenderer(HandleEvents, HandelRender, ScreenWidth, ScreenHeight);
    private static void Main(string[] args) => _renderer.Run(60);
    private static Simulation _simulation = new Simulation(1000);

    private static void HandelRender(RenderArgs args)
    {
        _simulation.Update(args);
        _simulation.Display(args);
    }

    private static void HandleEvents(SDL_Event events)
    {

    }
}

internal class Simulation
{
    private Base _targetBaseA = new Base(BaseItentifier.Yellow, 100, 100);
    private Base _targetBaseB = new Base(BaseItentifier.Blue, 400, 400);
    private List<Agent> _agents = new List<Agent>();

    public Simulation(int agentCount)
    {
        for (int i = 0; i < agentCount; i++)
        {
            int x = new Random().Next(0, Program.ScreenWidth);
            int y = new Random().Next(0, Program.ScreenHeight);
            int speed = new Random().Next(2, 5);
            int direction = new Random().Next(0, 360);
            BaseItentifier target = new Random().Next(0, 2) == 0 ? _targetBaseA.Identifier : _targetBaseB.Identifier;
            _agents.Add(new Agent(_targetBaseA.Identifier, _targetBaseB.Identifier, target, x, y, speed, direction));
        }
    }

    public void Update(RenderArgs args)
    {
        foreach (var agent in _agents)
        {


            //1) take a step
            var newX = (int)(Math.Cos(agent.Direction) * agent.Speed);
            var newY = (int)(Math.Sin(agent.Direction) * agent.Speed);


            //if this step would take the agent outside the screen, do a 180
            if (agent.X + newX < 0 || agent.X + newX > Program.ScreenWidth)
            {
                agent.Direction += 180;
            }

            if (agent.Y + newY < 0 || agent.Y + newY > Program.ScreenHeight)
            {
                agent.Direction += 180;
            }

            agent.X += newX;
            agent.Y += newY;

            //2) Increase both counters by 1
            agent.TargetACouter++;
            agent.TargetBCouter++;

            //3) IF the agent bumps into target A or B
            //    Reset the respective counter to 0
            //        IF this is the dest target for the agent, do a 180 and change the dest target

            bool isAtTargetA = Math.Abs(agent.X - _targetBaseA.X) < Base.Radius && Math.Abs(agent.Y - _targetBaseA.Y) < Base.Radius;
            bool isAtTargetB = Math.Abs(agent.X - _targetBaseB.X) < Base.Radius && Math.Abs(agent.Y - _targetBaseB.Y) < Base.Radius;

            if (isAtTargetA)
            {
                agent.TargetACouter = 0;
                if (agent.CurrentTarget == _targetBaseA.Identifier)
                {
                    agent.Direction += 180;
                    agent.CurrentTarget = _targetBaseB.Identifier;
                }
            }

            if (isAtTargetB)
            {
                agent.TargetBCouter = 0;
                if (agent.CurrentTarget == _targetBaseB.Identifier)
                {
                    agent.Direction += 180;
                    agent.CurrentTarget = _targetBaseA.Identifier;
                }
            }

            //4) Shout the value of one of the counters, plus the max shouting range
            var agentsWithinShoutingRange = _agents.Where(a => a != agent && Math.Abs(a.X - agent.X) < agent.ShoutRange && Math.Abs(a.Y - agent.Y) < agent.ShoutRange);

            var distanceAShout = agent.TargetACouter + agent.ShoutRange;
            var distanceBShout = agent.TargetBCouter + agent.ShoutRange;



            foreach (var otherAgent in agentsWithinShoutingRange)
            {
                if (otherAgent.TargetACouter > distanceAShout)
                {
                    otherAgent.TargetACouter = distanceAShout;
                    if (agent.CurrentTarget == _targetBaseA.Identifier)
                    {
                        Drawer.DrawLine(args, agent.X, agent.Y, otherAgent.X, otherAgent.Y, agent.CurrentTarget);
                        agent.Direction = otherAgent.Direction;
                    }
                }
                else if (otherAgent.TargetBCouter > distanceBShout)
                {
                    otherAgent.TargetBCouter = distanceBShout;
                    if (agent.CurrentTarget == _targetBaseB.Identifier)
                    {
                        Drawer.DrawLine(args, agent.X, agent.Y, otherAgent.X, otherAgent.Y, agent.CurrentTarget);
                        agent.Direction = otherAgent.Direction;
                    }
                }
            }

        }
    }

    public void Display(RenderArgs args)
    {
        _targetBaseA.Render(args);
        _targetBaseB.Render(args);
        foreach (var agent in _agents) agent.Render(args);

    }
}



public class Agent
{
    public Agent(BaseItentifier targetA, BaseItentifier targetB, BaseItentifier initalTarget, int x, int y, int speed, int direction)
    {
        TargetA = targetA;
        TargetB = targetB;
        X = x;
        Y = y;
        Speed = speed;
        Direction = direction;
        CurrentTarget = initalTarget;
    }

    public int X { get; set; }
    public int Y { get; set; }
    public double Speed { get; init; }
    public double Direction { get; set; }
    public int ShoutRange { get; init; } = 10;
    public int TargetACouter { get; set; } = 0;
    public int TargetBCouter { get; set; } = 0;
    public BaseItentifier TargetA { get; set; }
    public BaseItentifier TargetB { get; set; }
    public BaseItentifier CurrentTarget { get; set; }
    public void Render(RenderArgs args) => Drawer.DrawDot(args, X, Y, CurrentTarget.ToColor());
}

public class Base
{
    public Base(BaseItentifier identifier, int x, int y)
    {
        Identifier = identifier;
        X = x;
        Y = y;
    }

    public BaseItentifier Identifier { get; init; }
    public int X { get; set; }
    public int Y { get; set; }
    public const int Radius = 20;

    public void Render(RenderArgs args) => Drawer.DrawCircle(args, X, Y, Radius, Identifier.ToColor());
}

public enum BaseItentifier
{
    Red,
    Blue,
    Green,
    Yellow
}

