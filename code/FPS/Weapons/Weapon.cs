public abstract class Weapon : Component, Component.ICollisionListener
{

	public abstract float GetViewmodelXOffset();
	public abstract float GetViewmodelYOffset();
	public abstract float GetViewmodelZOffset();
	public abstract void Fire();
	public abstract void Reload();

	protected override void OnUpdate()
	{

	}

	public void OnCollisionUpdate(Collision collision)
	{
		Log.Info($"Collision occurred: {collision}");

	}

}
