using System.Collections.Generic;

namespace eCombat.Model
{
    public class Army
    {
        public static List<BoardPiece> GetEnemyList()
        {
            var enemyList = new List<BoardPiece>();

            for (int i = 0; i < 40; i++)
            {
                var newEnemy = new BoardPiece(null, moveLevel: -1, isEnemy: true);
                enemyList.Add(newEnemy);
            }

            return enemyList;
        }

        public static List<BoardPiece> GetUnitlist()
        {
            var unitList = new List<BoardPiece>();

            for (int i = 0; i < 1; i++)
            {
                var novoPrisioneiro = new BoardPiece("*");
                unitList.Add(novoPrisioneiro);
            }

            for (int i = 0; i < 6; i++)
            {
                var novaBomba = new BoardPiece("0");
                unitList.Add(novaBomba);
            }

            for (int i = 0; i < 1; i++)
            {
                var novoEspiao = new BoardPiece("1");
                unitList.Add(novoEspiao);
            }

            for (int i = 0; i < 8; i++)
            {
                var novoSoldado = new BoardPiece("2", moveLevel: 9);
                unitList.Add(novoSoldado);
            }

            for (int i = 0; i < 5; i++)
            {
                var novoCaboArmeiro = new BoardPiece("3");
                unitList.Add(novoCaboArmeiro);
            }

            for (int i = 0; i < 4; i++)
            {
                var novoSargento = new BoardPiece("4");
                unitList.Add(novoSargento);
            }

            for (int i = 0; i < 4; i++)
            {
                var novoTenente = new BoardPiece("5");
                unitList.Add(novoTenente);
            }

            for (int i = 0; i < 4; i++)
            {
                var novoCapitao = new BoardPiece("6");
                unitList.Add(novoCapitao);
            }

            for (int i = 0; i < 3; i++)
            {
                var novoMajor = new BoardPiece("7");
                unitList.Add(novoMajor);
            }

            for (int i = 0; i < 2; i++)
            {
                var novoCoronel = new BoardPiece("8");
                unitList.Add(novoCoronel);
            }

            for (int i = 0; i < 1; i++)
            {
                var novoGeneral = new BoardPiece("9");
                unitList.Add(novoGeneral);
            }

            for (int i = 0; i < 1; i++)
            {
                var novoMarechal = new BoardPiece("10");
                unitList.Add(novoMarechal);
            }

            return unitList;
        }
    }
}
