# Headway Log

Calculate departure gaps by service day, stop, route and direction, including after midnight.

## Try it

From this folder with the .NET 10 SDK installed:

```sh
dotnet run -- service study.tsv
```

Rows are service date, stop, route, direction and elapsed service time HH:mm (00:00–99:59). Use 24:05 for five minutes after midnight on the same service day. Enter each group in running order.

The included study.tsv uses tab-separated fields. Comment lines start with `#`.

The original exercise remains available with `dotnet run -- example.tsv`.

## When to use it

A transport student compares regularity at a stop where the route runs past midnight.

Example result:

```text
2026-05-01 Main N1 out: gaps 10 min; expected wait 5 min
```

Related course or field workflow: [GTFS service-day time specification](https://gtfs.org/documentation/schedule/reference/).
