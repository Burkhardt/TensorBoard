# TensorBoard support for MLW/C8Y

## Idea

We don't really need the Tensorboard Server App ... which also needs all Python stuff installed in the same docker image where the MLW is running   

TODO <i>As a data scientist, I need to see my models training progress in a Dashboard and receive Alarms, when the model doe not improve anymore</i>

	McNabb:RaiUtils20180918 RSB$ tensorboard -helpfull
	/Users/RSB/anaconda/lib/python3.6/site-packages/h5py/__init__.py:34: FutureWarning: Conversion of the second argument of issubdtype from `float` to `np.floating` is deprecated. In future, it will be treated as `np.float64 == np.dtype(float).type`.
		from ._conv import register_converters as _register_converters

       USAGE: /Users/RSB/anaconda/bin/tensorboard [flags]
flags:

tensorboard.plugins.debugger.debugger_plugin_loader:
  --debugger_data_server_grpc_port: The port at which the non-interactive debugger data server should receive debugging data via gRPC from one or more debugger-enabled TensorFlow runtimes. No debugger plugin or debugger data server will be started if this flag is not provided. This flag differs
    from the `--debugger_port` flag in that it starts a non-interactive mode. It is for use with the "health pills" feature of the Graph Dashboard. This flag is mutually exclusive with `--debugger_port`.
    (default: '-1')
    (an integer)
  --debugger_port: The port at which the interactive debugger data server (to be started by the debugger plugin) should receive debugging data via gRPC from one or more debugger-enabled TensorFlow runtimes. No debugger plugin or debugger data server will be started if this flag is not provided.
    This flag differs from the `--debugger_data_server_grpc_port` flag in that it starts an interactive mode that allows user to pause at selected nodes inside a TensorFlow Graph or between Session.runs. It is for use with the interactive Debugger Dashboard. This flag is mutually exclusive with
    `--debugger_data_server_grpc_port`.
    (default: '-1')
    (an integer)

tensorboard.plugins.profile.profile_plugin:
  --master_tpu_unsecure_channel: IP address of "master tpu", used for getting streaming trace data through tpu profiler analysis grpc. The grpc channel is not secured.
    (default: '')

tensorboard.program:
  --db: [Experimental] Sets SQL database URI.

    This mode causes TensorBoard to persist experiments to a SQL database. The
    following databases are supported:

    - sqlite: Use SQLite built in to Python. URI must specify the path of the
    database file, which will be created if it doesn't exist. For example:
    --db sqlite:~/.tensorboard.db

    Warning: This feature is a work in progress and only has limited support.
    (default: '')
  --event_file: The particular event file to query for. Only used if --inspect is present and --logdir is not specified.
    (default: '')
  --host: What host to listen to. Defaults to serving on all interfaces, set to 127.0.0.1 (localhost) to disable remote access (also quiets security warnings).
    (default: '')
  --[no]inspect: Use this flag to print out a digest
    of your event files to the command line, when no data is shown on TensorBoard or
    the data shown looks weird.

    Example usages:
    tensorboard --inspect --event_file myevents.out
    tensorboard --inspect --event_file myevents.out --tag loss
    tensorboard --inspect --logdir mylogdir
    tensorboard --inspect --logdir mylogdir --tag loss

    See tensorflow/python/summary/event_file_inspector.py for more info and
    detailed usage.
    (default: 'false')
  --logdir: logdir specifies the directory where
    TensorBoard will look to find TensorFlow event files that it can display.
    TensorBoard will recursively walk the directory structure rooted at logdir,
    looking for .*tfevents.* files.

    You may also pass a comma separated list of log directories, and TensorBoard
    will watch each directory. You can also assign names to individual log
    directories by putting a colon between the name and the path, as in

    tensorboard --logdir name1:/path/to/logs/1,name2:/path/to/logs/2
    (default: '')
  --max_reload_threads: The max number of threads that TensorBoard can use to reload runs. Not relevant for db mode. Each thread reloads one run at a time.
    (default: '1')
    (an integer)
  --path_prefix: An optional, relative prefix to the path, e.g. "/path/to/tensorboard". resulting in the new base url being located at localhost:6006/path/to/tensorboard under default settings. A leading slash is required when specifying the path_prefix, however trailing slashes can be omitted.
    The path_prefix can be leveraged for path based routing of an elb when the website base_url is not available e.g. "example.site.com/path/to/tensorboard/"
    (default: '')
  --port: What port to serve TensorBoard on.
    (default: '6006')
    (an integer)
  --[no]purge_orphaned_data: Whether to purge data that may have been orphaned due to TensorBoard restarts. Disabling purge_orphaned_data can be used to debug data disappearance.
    (default: 'true')
  --reload_interval: How often the backend should load more data, in seconds. Set to 0 to load just once at startup and a negative number to never reload at all.
    (default: '5')
    (an integer)
  --samples_per_plugin: An optional comma separated list of plugin_name=num_samples pairs to explicitly specify how many samples to keep per tag for that plugin. For unspecified plugins, TensorBoard randomly downsamples logged summaries to reasonable values to prevent out-of-memory errors for
    long running jobs. This flag allows fine control over that downsampling. Note that 0 means keep all samples of that type. For instance, "scalars=500,images=0" keeps 500 scalars and all images. Most users should not need to set this flag.
    (default: '')
  --tag: The particular tag to query for. Only used if --inspect is present
    (default: '')
  --window_title: The title of the browser window.
    (default: '')

tensorflow.python.platform.app:
  -h,--[no]help: show this help
    (default: 'false')
  --[no]helpfull: show full help
    (default: 'false')
  --[no]helpshort: show this help
    (default: 'false')

absl.flags:
  --flagfile: Insert flag definitions from the given file into the command line.
    (default: '')
  --undefok: comma-separated list of flag names that it is okay to specify on the command line even if the program does not define a flag with that name.  IMPORTANT: flags in this list that have arguments MUST use the --flag=value format.
    (default: '')