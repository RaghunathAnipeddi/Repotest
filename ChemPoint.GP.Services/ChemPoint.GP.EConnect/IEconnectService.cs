using System.ServiceModel;

namespace ChemPoint.GP.EConnect
{
    [ServiceContract]
    public interface IEconnectService
    {
        [OperationContract]
        bool ProcessOrderMessage(string eComOrderXml, int iterationNumber);
    }
}
