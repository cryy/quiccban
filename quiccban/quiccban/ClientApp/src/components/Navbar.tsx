﻿import * as React from 'react';
import { createStyles, makeStyles, Theme } from '@material-ui/core/styles';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import Typography from '@material-ui/core/Typography';
import IconButton from '@material-ui/core/IconButton';
import MenuIcon from '@material-ui/icons/Menu';
import MenuItem from '@material-ui/core/MenuItem';
import Menu from '@material-ui/core/Menu';
import { Avatar, Divider } from '@material-ui/core';
import { NavbarProps } from '../props/NavbarProps';

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            flexGrow: 1,
        },
        menuButton: {
            marginRight: theme.spacing(2),
        },
        title: {
            flexGrow: 1,
        },
        appBar: {
            backgroundColor: theme.palette.background.paper,
            position: "relative",
            top: "0%",
            left: "0%"
        },
    }),
);

export default function Navbar(props: NavbarProps) {
    const classes = useStyles({});
    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);


    function handleMenu(event: React.MouseEvent<HTMLElement>) {
        setAnchorEl(event.currentTarget);
    }

    function handleClose() {
        setAnchorEl(null);
    }

    if (props.user) {
        return (
            <div className={classes.root}>
                <AppBar elevation={0} classes={{ root: classes.appBar }} color="default">
                    <Toolbar>
                        <IconButton edge="start" className={classes.menuButton} color="inherit" aria-label="Menu">
                            <MenuIcon />
                        </IconButton>
                        <Typography variant="h6" className={classes.title}>
                            quiccban
                        </Typography>

                        <div>
                            <IconButton
                                aria-label="Account of current user"
                                aria-controls="menu-appbar"
                                aria-haspopup="true"
                                onClick={handleMenu}
                                color="inherit"
                            >
                                <Avatar
                                    src={props.user.user.avatarUrl}
                                    style={{
                                        width: "45px",
                                        height: "45px",
                                    }}
                                />
                            </IconButton>
                            <Menu
                                id="menu-appbar"
                                anchorEl={anchorEl}
                                getContentAnchorEl={null}
                                anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
                                transformOrigin={{ vertical: "top", horizontal: "center" }}
                                open={open}
                                onClose={handleClose}
                            >
                                <Typography>{props.user.user.username}#{props.user.user.discriminator}</Typography>
                                <Divider />
                                <MenuItem onClick={(e) => window.location.href = "/api/auth/logout"}>Logout</MenuItem>
                            </Menu>
                        </div>

                    </Toolbar>
                </AppBar>
            </div>
        );
    }
    else return <div></div>;
}