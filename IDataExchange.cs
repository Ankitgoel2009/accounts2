using System;

public interface IDataExchange
{
    /// <summary>
    /// Receives data from other forms/UserControls
    /// </summary>
    /// <param name="data">Data to be received</param>
    void ReceiveData(object data);

    /// <summary>
    /// Returns data to be passed to other forms/UserControls
    /// </summary>
    /// <returns>Data to be passed</returns>
    object GetData();
}