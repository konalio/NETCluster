namespace DVRPTaskSolver
{
    public class DVRPTaskSolverCreator : UCCTaskSolver.TaskSolverCreator
    {
        public override UCCTaskSolver.TaskSolver CreateTaskSolverInstance(byte[] problemData)
        {
            return new DVRPTaskSolver(problemData);
        }
    }
}
