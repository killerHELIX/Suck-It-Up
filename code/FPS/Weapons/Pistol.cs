
public sealed class Pistol : Component, IWeapon
{
    public void fire()
    {
        throw new System.NotImplementedException();
    }

    public void reload()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
		}
	}
}