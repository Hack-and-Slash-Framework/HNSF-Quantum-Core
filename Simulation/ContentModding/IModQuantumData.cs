using Quantum;

public interface IModQuantumData {
    public int GetComponentTypeIdCount();
    public ComponentTypeId.Builder InitComponentTypeIdGen(ref ComponentTypeId.Builder builder); 
    public void InitStaticDelegates();
    public void RegisterSimulationTypesGen(TypeRegistry typeRegistry);

    public void InitGen(Quantum.Frame frame);
}
