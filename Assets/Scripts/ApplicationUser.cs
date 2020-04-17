using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ApplicationUser
{
    public Player player { get; set; }
    public string Name { get; set; }
    public int Chips { get; set; }
    public List<string[]> PlayerCards { get; set; }
    public bool IsPlayingThisRound { get; set; } = true;
    public bool IsPlayingThisGame { get; set; } = true;
}
