namespace System
{
    public delegate void EventHandler2<TEventArgs>(Object sender, TEventArgs e); // Removed TEventArgs constraint post-.NET 4
}
