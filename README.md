# Asura

Rocky planets for [Aetheria](https://github.com/GameCult/Aetheria), grown from a field.

A planet's shape is one function. Surface nets turns it into quads, each quad gets
its own tile, and a compute pass fills every tile with detail refined onto the
same function, so the displaced surface stays true to the field and the dunes
reach the silhouette. The tiles are a cache; the field is the truth.

Status: target written, cuts not yet mapped. See [`docs/target.md`](docs/target.md).
