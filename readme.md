# Seaq

### Simple Elasticsearch Application Queries

![develop:push](https://github.com/oklacoder/seaq/workflows/develop:push/badge.svg)

![master:push](https://github.com/oklacoder/seaq/workflows/master:push/badge.svg)

## Contents

- [Introduction](#introduction)
- [Core Concepts](#core-concepts)
- [Cluster](#cluster)

## <a id="introduction">Introduction</a>

Seaq is an opinionated wrapper for the first-party Elasticsearch dotnet libraries.  It uses those libraries and expands upon them - it sets what are intended to be sensible defaults and provides consistent pathways for querying, indexing, and deleting documents.  It provides a set of index management utilities, as well as shortcuts for source filtering, custom query scoring, and more.

## <a id="core-concepts">Core Concepts</a>

<br/>

### <a id="cluster">Cluster</a>

<hr/>

Seaq's operation is based on the idea of a `Cluster`.  A seaq Cluster is, at its base, meant to be roughly analogous to the physical Elasticsearch cluster.  It supports the obvious actions like `CreateIndex` and `Query`, but also has some more idiosyncratic functionality like `IncludeIndexInGlobalSearch` and `SetndexSecondaryFieldLabel`.  This document is intended to provide an exhaustive list of these options.

Each seaq `cluster` has a `scope` - this value should be unique, and should be easily recognizable.  This `scope` value, provided at `cluster` creation, is used to identify the indices that are tied to the `cluster` and that the `cluster` should track.  The default value for most seaq index names is `{cluster scope}_{fully-qualified dotnet type name}`.  This default is intended to be descriptive, intuitive, and to reduce code complexity re: consistently tracking and constructing names.

A developer can create a `cluster` by calling the static `seaq.Cluster.Create(`ClusterArgs args`)` method.  This method returns a functioning `Cluster` object with its internal index cache populated with all available server indices that match the provided `scope`.  Ie a `cluster` created with `scope` "dev_server" would cache the existence, as well as settings for, indices "dev_server_Namespace.TypeName", but not "test_serer_Namespace.TypeName".  

### <a id="index">Index</a>

<hr/>

A seaq `index` is a direct representation of an Elasticsearch index.  Each seaq `index` is mapped to a dotnet type - you'll want fully-qualified `Type.FullName` values as index names in most circumstances.  