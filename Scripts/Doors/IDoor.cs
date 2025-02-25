using System.Collections;

//mainly used to simplify referncing doors in the scene

public interface IDoor
{
    IEnumerator OpenDoor();
    void CloseDoor();
}
