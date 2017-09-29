# la-nina

La Niña is the name of one of the boat from Columbus when discovering America. So the ellipsis to a new world or a big mistake (up to your feeling).

**Goal : Enable E2E HPC workload on Azure**

The HPC cluster is provisionned from an ARM template, it will have a single jumpbox and a set of nodes deployed thru a VM Scaleset. The automation script will drive actions on the jumpbox thru SSH.


## Prerequisites
Linux is the only supported platform to run the automation scripts


Setup packages

    yum --enablerepo=extras install -y -q epel-release
    yum install -y git jq xmllint curl wget bc

Install Azure CLI 2.0 from [here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) or type the following for CentOS:

    sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
    sudo sh -c 'echo -e "[azure-cli]\nname=Azure CLI\nbaseurl=https://packages.microsoft.com/yumrepos/azure-cli\nenabled=1\ngpgcheck=1\ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/azure-cli.repo'
    yum check-update
    sudo yum -y install azure-cli


    az login
    az account set --subscription <id>
    az account list

create sshkey

    ssh-keygen

update ssh config

    cat << EOF > /home/$USER/.ssh/config
    Host *
        StrictHostKeyChecking no
        UserKnownHostsFile /dev/null
        PasswordAuthentication no
    EOF

    chmod 644 /home/$USER/.ssh/config


Clone the repo with the scripts

    git clone https://github.com/edwardsp/azhpc.git


## parameter file

There are few parameter files provided in the params directory, use them as a starting point but the main two are :
* _ncus-h16r-centos71hpc.tpl_ for North Central US
* _we-h16r-centos71hpc.tpl_ for West Europe

copy one of them into the azhpc directory you just cloned

### OpenFOAM Benchmark Parameters

There are two OpenFOAM scripts in the azhpc repo in order to separate decomposition to running the solvers.  The scripts are general to allow different benchmarks but the following parameters are required:

    storageAccountName=paedwar
    storageContainerName=azhpc
    storageBenchmarkPath=benchmarks
    storageBenchmarkName=motorbike82M
    storageSasKey='?sv=2016-05-31&si=azhpc-rw&sr=c&sig=SIj%2BwhmwdjsstxykYMu8tfnx8qbCAUKnD1HlCwVnfI4%3D'

This is for the large testcase.

## run 

from the azhpc directory execute the following command to run the single node HPL test

    ./automation/deploy.sh  ncus-h16r-centos71hpc.tpl ./automation/singleNodeHPL.sh
