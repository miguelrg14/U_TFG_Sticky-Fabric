"""Plot the headless simulation output for the README.

Usage:
    dotnet run --project tools/StickyFabric.Simulation -- --csv build/simulation
    python tools/plot_simulation.py build/simulation docs/images/simulation.png
"""
import csv
import sys
from collections import defaultdict

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402

GUM = "#d0479b"
INK = "#2b2b33"


def read_rows(path):
    with open(path, newline="", encoding="utf-8") as handle:
        return list(csv.DictReader(handle))


def main(source_dir, output_path):
    profile = defaultdict(list)
    for row in read_rows(f"{source_dir}/profile_961.csv"):
        profile[float(row["time"])].append((float(row["x"]), float(row["z"])))

    timeline = read_rows(f"{source_dir}/timeline_961.csv")

    fig, (left, right) = plt.subplots(1, 2, figsize=(11, 3.8), dpi=110)

    wanted = [0.2, 0.4, 0.6, 0.8, 0.9, 1.0]
    picks = [min(profile, key=lambda t, w=w: abs(t - w)) for w in wanted]
    colours = plt.cm.RdPu([0.35 + 0.65 * i / (len(picks) - 1) for i in range(len(picks))])
    for colour, time in zip(colours, picks):
        points = sorted(profile[time])
        left.plot([p[0] for p in points], [p[1] for p in points], color=colour, linewidth=1.8, label=f"t = {time:.1f} s")
    left.set_title("Middle row of the fabric while the sphere pulls away", color=INK)
    left.set_xlabel("x (local units)")
    left.set_ylabel("z displacement (local units)")
    left.axhline(-0.5, color=INK, linestyle=":", linewidth=1)
    left.text(0.47, -0.48, "elastic limit", ha="right", va="bottom", fontsize=8, color=INK)
    left.legend(frameon=False, fontsize=8)

    t = [float(r["time"]) for r in timeline]
    right.plot(t, [float(r["max_displacement"]) for r in timeline], color=GUM, linewidth=2, label="max stretch")
    right.set_xlabel("time (s)")
    right.set_ylabel("max stretch (local units)", color=GUM)
    right.set_title("961-vertex fabric: stick, stretch, snap, recover", color=INK)
    glued_axis = right.twinx()
    glued_axis.plot(t, [int(r["glued"]) for r in timeline], color=INK, linewidth=1.2, linestyle="--", label="glued vertices")
    glued_axis.set_ylabel("glued vertices", color=INK)

    for axis in (left, right, glued_axis):
        for side in ("top",):
            axis.spines[side].set_visible(False)

    fig.tight_layout()
    fig.savefig(output_path)
    print(f"Wrote {output_path}")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
