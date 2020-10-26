# RoboSharp Contributing Guidelines

Hey there! I'm really glad that you are interested in contributing to RoboSharp! Here are some hopefully straight-forward guidelines to do so.
=======

# Pull Requests

Please don't submit PRs against the `master` branch as it is only a snapshot of the latest stable release. Instead, please submit all PRs against the `dev` branch where all active development is taking place. You can merge back directly into the `dev` branch as well.

In addition to the `dev` branch, please also merge changes to the `NET3.5` branch (where applicable) to keep that legacy library up to date as well.

If you are adding a new feature, please add a test case (coming soon, hopefully ;). 

If you are fixing a bug, please add `(fix #xxx)` to the PR title. Try and provide a detailed description of the bug.

# Project

We will be using projects going forward to plan for the next release. Please make sure that any feature or bug you are adding is added to the correct project as well which will, usually, just be the next release.

Thanks Everyone :)
