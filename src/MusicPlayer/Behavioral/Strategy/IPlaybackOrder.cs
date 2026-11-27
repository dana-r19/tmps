using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Strategy;

// =====================================================================================
// STRATEGY PATTERN - overview
// -------------------------------------------------------------------------------------
// Intent: define a family of interchangeable algorithms, put each one in its own class,
//         and let the client choose one at runtime.
//
// Roles in this project:
//   Strategy           -> IPlaybackOrder (this file)
//   Concrete strategies-> SequentialOrder, ReverseOrder, ShuffleOrder, ShortestFirstOrder
//   Context            -> PlayQueue (keeps a reference to the current IPlaybackOrder)
//
// Problem it solves here: the user can choose HOW the queue is played (in order, reversed,
// shuffled, shortest first) and can change it in the middle of a session. Without Strategy,
// PlayQueue would need "if (mode == shuffle) ... else if (mode == reverse) ..." and every new
// order would mean editing PlayQueue.
//
// How it works: PlayQueue only calls order.Arrange(items). It never asks WHICH order it has,
// so swapping the order is a single assignment, and a new order is a new class only.
// =====================================================================================

/// <summary>
/// STRATEGY - the strategy interface. Decides in which order the queue is played.
/// Every order has this same signature, so PlayQueue can use any of them without knowing which.
/// </summary>
public interface IPlaybackOrder
{
    /// <summary>Shown to the user (and in the command history), e.g. "shuffle".</summary>
    string Name { get; }

    /// <summary>
    /// The algorithm itself: returns the items in the order they should be played.
    /// Must return a NEW list and leave the input untouched, because PlayQueue keeps the
    /// original list and re-arranges it every time the strategy is swapped.
    /// </summary>
    IReadOnlyList<IPlayable> Arrange(IReadOnlyList<IPlayable> items);
}
