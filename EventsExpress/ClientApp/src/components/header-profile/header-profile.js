import React, {Component} from "react";
import IconButton from "@material-ui/core/IconButton";
import Create from "@material-ui/icons/Create";
import Notifications from "@material-ui/icons/Notifications";
import DirectionsRun from "@material-ui/icons/DirectionsRun";
import AccountCircle from "@material-ui/icons/AccountCircle";
import Switch from "@material-ui/core/Switch";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import FormGroup from "@material-ui/core/FormGroup";
import MenuItem from "@material-ui/core/MenuItem";
import Menu from "@material-ui/core/Menu";
import CustomAvatar from '../avatar/custom-avatar';
import ModalWind from '../modal-wind';
import { Link } from 'react-router-dom';
import RatingAverage from '../rating/rating-average'
import Badge from '@material-ui/core/Badge';


import './header-profile.css';

export default class HeaderProfile extends Component {


    render(){
 
        const { id, name, photoUrl, email, rating } = this.props.user;
        const { onClick } = this.props;
    
    return (
        <div className='root'>
            <div>
                {!id && (
                    <ModalWind  reset={this.props.reset} />
                )}
                {id && (
                    <div className="d-flex flex-column align-items-center">

                        <CustomAvatar size="big" photoUrl={this.props.user.photoUrl} name={this.props.user.name} />
                        
                        <h4>{name}</h4>
                        <RatingAverage value={rating} direction='row' />
                        
                        <div>
                            <Link to={'/profile' }><IconButton
                                aria-label="Account of current user"
                                aria-controls="menu-appbar"
                                aria-haspopup="true"
                            >
                                <Create />
                            </IconButton></Link>
                            <Link to={'/notification_events' }>
                            <IconButton
                                aria-label="Account of current user"
                                aria-controls="menu-appbar"
                                aria-haspopup="true"
                            >
                            <Badge badgeContent={this.props.notification} color="primary">
                                <Notifications />
                            </Badge>
                            </IconButton>
                                </Link>
                            <Link to="/home/events?page=1">
                            <IconButton
                                className='menuButton'
                                aria-label="Exit"
                                aria-controls="menu-appbar"
                                aria-haspopup="true"
                                onClick={onClick}
                            >
                                <DirectionsRun />
                                </IconButton>
                            </Link>
                        </div>
                        
                            
                    </div>
                )}
            </div>
        </div>
    );
}
}