﻿namespace Divan
{
    public interface ICouchViewDefinitionBase
    {
        ICouchDatabase Db();
        CouchDesignDocument Doc { get; set; }
        string Name { get; set; }
        string Path();
        ICouchRequest Request();
    }
}
