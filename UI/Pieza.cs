using OpenTK.Mathematics;

namespace Tetris2D.UI
{
    public enum TipoPieza
    {
        I, J, L, O, S, T, Z
    }

    public class Pieza
    {
        public IReadOnlyList<(int x, int y)> Bloques { get; private set; }
        public TipoPieza Tipo { get; }
        public Vector4 Color { get; }

    private static readonly IReadOnlyDictionary<TipoPieza, (int x, int y)[]> FormasBase =
        new Dictionary<TipoPieza, (int x, int y)[]>
        {
            { TipoPieza.I, new[] { (0, 1), (1, 1), (2, 1), (3, 1) } },
            { TipoPieza.O, new[] { (1, 1), (2, 1), (1, 2), (2, 2) } },
            { TipoPieza.T, new[] { (1, 0), (0, 1), (1, 1), (2, 1) } },
            { TipoPieza.S, new[] { (1, 0), (2, 0), (0, 1), (1, 1) } },
            { TipoPieza.Z, new[] { (0, 0), (1, 0), (1, 1), (2, 1) } },
            { TipoPieza.J, new[] { (0, 0), (0, 1), (1, 1), (2, 1) } },
            { TipoPieza.L, new[] { (2, 0), (0, 1), (1, 1), (2, 1) } }
        };

        private static readonly IReadOnlyDictionary<TipoPieza, Vector4> Colores =
        new Dictionary<TipoPieza, Vector4>
        {
            { TipoPieza.I, TemaArcade.Cian },
            { TipoPieza.O, TemaArcade.Amarillo },
            { TipoPieza.T, TemaArcade.Magenta },
            { TipoPieza.S, TemaArcade.Verde },
            { TipoPieza.Z, TemaArcade.Rojo },
            { TipoPieza.J, TemaArcade.Azul },
            { TipoPieza.L, TemaArcade.Naranja }
        };

        public Pieza(TipoPieza tipo)
        {
            Tipo = tipo;
            Bloques = FormasBase[tipo];
            Color = Colores[tipo];
        }

        public List<(int x, int y)> ObtenerRotacion(bool horario)
        {
            if (Tipo == TipoPieza.O)
                return Bloques.ToList();

            var resultado = new List<(int x, int y)>(Bloques.Count);
            foreach (var (x, y) in Bloques)
            {
                //Rotacion 90° horario en la caja 4x4: (x, y) -> (3-y, x).
                //Antihorario en su inverrsa: (x, y) -> (y, 3-x).
                resultado.Add(horario ? (3 - y, x) : (y, 3 - x));
            }
            return resultado;
        }

        public void AplicarRotacion(bool horario)
        {
            Bloques = ObtenerRotacion(horario);
        }

        public static Pieza Aleatoria(Random rnd)
        {
            TipoPieza tipo = (TipoPieza)rnd.Next(0, 7);
            return new Pieza(tipo);
        }
    }
}