using System.ServiceModel;

namespace GameServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IGameMasterSvc" in both code and config file together.
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IGameMasterSvc
    {
        [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
        [FaultContract(typeof(GameMasterSvcFault))]
        void EnterGame(string clientId);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = true)]
        void LeaveGame();

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void FaceMatch(string displayName);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void CancelMatch();

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void MoveBoardPiece(int srcX, int srcY, int destX, int destY);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void AttackBoardPiece(int srcX, int srcY, int destX, int destY, string attackerPowerLevel);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void WriteMessageToChat(string message);
    }
}
